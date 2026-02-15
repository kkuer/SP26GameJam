using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    [Header("Node Settings")]
    [SerializeField] private NodeType nodeType = NodeType.Ability;
    [SerializeField] private NodeState nodeState = NodeState.Draggable;

    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 10f;

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 15f;

    [Header("Line Renderer Settings")]
    [SerializeField] private int linePoints = 8; // Number of points per line (including start and end)
    [SerializeField] private AnimationCurve lineWidthCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.1f);
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;

    [Header("Preview Line Settings")]
    [SerializeField] private bool showPreviewWhileDragging = true;
    [SerializeField] private Color previewLineColor = new Color(1f, 1f, 0f, 0.5f); // Yellow semi-transparent
    [SerializeField] private float previewLineWidth = 0.15f;
    [SerializeField] private int maxPreviewLines = 6; // Maximum number of preview lines
    [SerializeField] private Material previewMaterial; // Optional: different material for preview

    [Header("Preview Pulse Settings")]
    [SerializeField] private float pulseSpeed = 5f; // Speed of the pulse effect
    [SerializeField] private float pulseIntensity = 0.2f; // How much the brightness varies (0 = no pulse, 1 = full variation)
    [SerializeField] private float pulseBaseAlpha = 0.5f; // Base alpha value (same as previewLineColor.a)

    [Header("Line Animation Settings")]
    [SerializeField] private float lineDrawSpeed = 2f; // Speed of line drawing animation
    [SerializeField] private bool animateLines = true; // Toggle animation on/off
    [SerializeField] private AnimationCurve drawEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Easing for animation

    private Vector3 targetPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 snapTarget;

    private NodeSlot currentSlot;
    private NodeSlot previewSlot; // The slot this node would snap to

    // Connection visualization
    private List<LineRenderer> connectionLines = new List<LineRenderer>();
    private Dictionary<NodeSlot, LineRenderer> activeConnections = new Dictionary<NodeSlot, LineRenderer>();

    // Preview lines
    private List<LineRenderer> previewLines = new List<LineRenderer>();
    private Dictionary<NodeSlot, LineRenderer> activePreviews = new Dictionary<NodeSlot, LineRenderer>();

    // Track which node owns each connection (to prevent duplicates)
    private static Dictionary<(Node, Node), LineRenderer> globalConnections = new Dictionary<(Node, Node), LineRenderer>();

    // Line animation tracking
    private Dictionary<LineRenderer, float> lineDrawStartTime = new Dictionary<LineRenderer, float>();
    private Dictionary<LineRenderer, bool> animationCompleted = new Dictionary<LineRenderer, bool>();
    private float lastSnapTime;

    // Public properties
    public NodeType Type => nodeType;
    public NodeState State => nodeState;
    public NodeSlot CurrentSlot => currentSlot;

    void Awake()
    {
        CreateLineRenderers();
        CreatePreviewLines();
    }

    void CreateLineRenderers()
    {
        // Create line renderer pool
        for (int i = 0; i < 6; i++) // Max 6 connections
        {
            GameObject lineObj = new GameObject($"Connection_{i}");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            // Set default material if none provided
            if (lineMaterial == null)
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
            else
            {
                lr.material = lineMaterial;
            }

            // Configure line renderer
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.positionCount = linePoints;
            lr.widthCurve = lineWidthCurve;
            lr.numCornerVertices = 5; // Smoother corners
            lr.numCapVertices = 5;     // Smoother ends
            lr.enabled = false;

            connectionLines.Add(lr);

            // Initialize tracking
            animationCompleted[lr] = false;
        }
    }

    void CreatePreviewLines()
    {
        // Create preview line pool
        for (int i = 0; i < maxPreviewLines; i++)
        {
            GameObject previewObj = new GameObject($"PreviewLine_{i}");
            previewObj.transform.SetParent(transform);
            previewObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = previewObj.AddComponent<LineRenderer>();

            // Set preview material
            if (previewMaterial != null)
            {
                lr.material = previewMaterial;
            }
            else if (lineMaterial != null)
            {
                lr.material = lineMaterial;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }

            // Configure preview line
            lr.startColor = previewLineColor;
            lr.endColor = previewLineColor;
            lr.startWidth = previewLineWidth;
            lr.endWidth = previewLineWidth;
            lr.positionCount = 2; // Simple straight line for preview
            lr.numCornerVertices = 5;
            lr.numCapVertices = 5;
            lr.enabled = false;

            previewLines.Add(lr);
        }
    }

    void Start()
    {
        // Only snap if not in inventory
        if (nodeState != NodeState.InInventory)
        {
            SnapToClosestAvailableSlot();
        }
    }

    void SnapToClosestAvailableSlot()
    {
        if (NodeManager.Instance == null)
        {
            Debug.LogError("NodeManager not found in scene!");
            return;
        }

        // Find the closest available node slot that matches this node's type
        NodeSlot closestSlot = NodeManager.Instance.GetClosestAvailableSlotOfType(transform.position, nodeType);

        if (closestSlot != null)
        {
            // Snap to slot position
            transform.position = closestSlot.transform.position;
            currentSlot = closestSlot;
            currentSlot.state = NodeSlotState.Occupied;
            currentSlot.OccupyingNode = this;

            // Update connections for this node and all neighbors
            UpdateConnections(true); // Pass true to indicate this is a new connection
            NotifyNeighborsToUpdateConnections();
        }
        else
        {
            Debug.LogWarning($"No available slots of type {nodeType} found for node!");
        }
    }

    void UpdatePreviewLines()
    {
        if (!showPreviewWhileDragging || !isDragging)
        {
            HideAllPreviews();
            return;
        }

        // Find the closest available slot (where this node would snap)
        if (NodeManager.Instance != null)
        {
            previewSlot = NodeManager.Instance.GetClosestAvailableSlotOfType(transform.position, nodeType);
        }

        if (previewSlot == null)
        {
            HideAllPreviews();
            return;
        }

        int previewIndex = 0;

        // Check all neighbors of that potential slot
        foreach (var neighborSlot in previewSlot.nearbyNodes)
        {
            if (previewIndex >= maxPreviewLines) break;

            // If neighbor exists and has an occupying node
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                // Check if this connection already exists in global connections
                var connectionKey = (this, neighborNode);
                var reverseKey = (neighborNode, this);

                // Only show preview for connections that don't already exist
                if (!globalConnections.ContainsKey(connectionKey) && !globalConnections.ContainsKey(reverseKey))
                {
                    LineRenderer previewLine = previewLines[previewIndex];
                    previewLine.enabled = true;

                    // Draw from the slot position (not the node position)
                    previewLine.SetPosition(0, previewSlot.transform.position);
                    previewLine.SetPosition(1, neighborNode.transform.position);

                    // Pulse effect while dragging - with separate speed and intensity controls
                    float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed + previewIndex) * pulseIntensity;
                    Color pulsedColor = previewLineColor;
                    pulsedColor.a = pulseBaseAlpha * pulse;
                    previewLine.startColor = pulsedColor;
                    previewLine.endColor = pulsedColor;

                    // Store in active previews
                    activePreviews[neighborSlot] = previewLine;

                    previewIndex++;
                }
            }
        }

        // Hide any unused preview lines
        for (int i = previewIndex; i < previewLines.Count; i++)
        {
            previewLines[i].enabled = false;
        }
    }

    void HideAllPreviews()
    {
        foreach (var preview in previewLines)
        {
            preview.enabled = false;
        }
        activePreviews.Clear();
        previewSlot = null;
    }

    void OnMouseDown()
    {
        // Check if node is in a state that allows dragging
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        offset = transform.position - GetMouseWorldPos();
        targetPosition = transform.position;
        isDragging = true;
        isSnapping = false;

        // Hide connections while dragging
        HideAllConnections();
        HideAllPreviews();

        // Remove this node's connections from global dictionary
        RemoveGlobalConnections();

        // If we're in a slot (not inventory), notify neighbors and free the slot
        if (currentSlot != null)
        {
            // Notify neighbors to update their connections (since this node is leaving)
            NotifyNeighborsToUpdateConnections();

            // Free up current slot
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;
            currentSlot = null;
        }
    }

    void OnMouseDrag()
    {
        // Check if node is in a state that allows dragging
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        targetPosition = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        // Check if node is in a state that allows dragging
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        isDragging = false;

        // Hide preview lines
        HideAllPreviews();

        // Handle different states on release
        switch (nodeState)
        {
            case NodeState.InInventory:
                // In inventory mode - just stay where released, no snapping
                nodeState = NodeState.Draggable;
                break;

            case NodeState.Draggable:
                // Find closest available slot that matches this node's type
                if (NodeManager.Instance != null)
                {
                    NodeSlot closestSlot = NodeManager.Instance.GetClosestAvailableSlotOfType(transform.position, nodeType);

                    if (closestSlot != null)
                    {
                        snapTarget = closestSlot.transform.position;
                        currentSlot = closestSlot;
                        isSnapping = true;
                    }
                    else
                    {
                        Debug.LogWarning($"No available slots of type {nodeType} found for node!");
                    }
                }
                break;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                dragSpeed * Time.deltaTime
            );

            // Update preview lines while dragging
            UpdatePreviewLines();
        }
        else if (isSnapping)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                snapTarget,
                snapSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                transform.position = snapTarget;
                isSnapping = false;
                lastSnapTime = Time.time;

                // Reset animation completion flags for new connections
                foreach (var lr in connectionLines)
                {
                    animationCompleted[lr] = false;
                }

                if (currentSlot != null)
                {
                    currentSlot.state = NodeSlotState.Occupied;
                    currentSlot.OccupyingNode = this;

                    // Update connections after snapping - pass true for new connection
                    UpdateConnections(true);
                    NotifyNeighborsToUpdateConnections();

                    // Notify NodeManager that state changed
                    if (NodeManager.Instance != null)
                    {
                        NodeManager.Instance.OnNodeStateChanged();
                    }
                }
            }
        }

        // Update line animations
        if (animateLines)
        {
            UpdateLineAnimations();
        }
    }

    void UpdateLinePoints(LineRenderer lr, Vector3 start, Vector3 end, float progress = 1f)
    {
        if (lr.positionCount < 2) return;

        Vector3[] points = new Vector3[lr.positionCount];

        // Apply easing to progress
        float easedProgress = drawEasingCurve.Evaluate(Mathf.Clamp01(progress));
        float totalDistance = Vector3.Distance(start, end);
        float drawnDistance = totalDistance * easedProgress;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)(lr.positionCount - 1);

            if (animateLines && progress < 1f)
            {
                // For animation, only draw up to the progress point
                float pointDistance = t * totalDistance;
                if (pointDistance <= drawnDistance)
                {
                    // Point is within drawn portion
                    points[i] = Vector3.Lerp(start, end, t);
                }
                else
                {
                    // Point is beyond drawn portion - clamp to end of drawn portion
                    float clampedT = drawnDistance / totalDistance;
                    points[i] = Vector3.Lerp(start, end, clampedT);
                }
            }
            else
            {
                // Full line
                points[i] = Vector3.Lerp(start, end, t);
            }
        }

        lr.SetPositions(points);
    }

    void UpdateLineAnimations()
    {
        List<(Node, Node)> toRemove = new List<(Node, Node)>();
        bool anyAnimationJustCompleted = false;

        foreach (var kvp in globalConnections)
        {
            var connection = kvp.Key;
            LineRenderer lr = kvp.Value;

            Node nodeA = connection.Item1;
            Node nodeB = connection.Item2;

            // Check if connection is still valid
            if (nodeA == null || nodeB == null || nodeA.gameObject == null || nodeB.gameObject == null)
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Check if nodes are still in adjacent slots
            if (nodeA.CurrentSlot == null || nodeB.CurrentSlot == null)
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Check if slots are still neighbors
            if (!AreSlotsNeighbors(nodeA.CurrentSlot, nodeB.CurrentSlot))
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Update line if renderer exists
            if (lr != null && lr.enabled)
            {
                // Determine which node is newer to animate from it
                float timeSinceSnapA = Time.time - nodeA.lastSnapTime;
                float timeSinceSnapB = Time.time - nodeB.lastSnapTime;

                if (timeSinceSnapA < timeSinceSnapB && timeSinceSnapA < 1f)
                {
                    // Animate from node A to node B
                    float progress = Mathf.Clamp01(timeSinceSnapA * lineDrawSpeed);
                    UpdateLinePoints(lr, nodeA.transform.position, nodeB.transform.position, progress);

                    // Check if animation just completed
                    if (progress >= 0.99f && !nodeA.animationCompleted[lr])
                    {
                        nodeA.animationCompleted[lr] = true;
                        anyAnimationJustCompleted = true;
                    }
                }
                else if (timeSinceSnapB < 1f)
                {
                    // Animate from node B to node A
                    float progress = Mathf.Clamp01(timeSinceSnapB * lineDrawSpeed);
                    UpdateLinePoints(lr, nodeB.transform.position, nodeA.transform.position, progress);

                    // Check if animation just completed
                    if (progress >= 0.99f && !nodeB.animationCompleted[lr])
                    {
                        nodeB.animationCompleted[lr] = true;
                        anyAnimationJustCompleted = true;
                    }
                }
                else
                {
                    // No animation, just draw full line
                    UpdateLinePoints(lr, nodeA.transform.position, nodeB.transform.position, 1f);
                }
            }
        }

        // Clean up invalid connections
        foreach (var connection in toRemove)
        {
            globalConnections.Remove(connection);
        }

        // --- ANIMATION COMPLETION HOOK ---
        // Add your logic here when any animation completes
        if (anyAnimationJustCompleted && NodeManager.Instance != null)
        {
            ShakeManager.Instance.shakeCam(2.5f, 1f, 0.5f);
        }
    }

    bool AreSlotsNeighbors(NodeSlot slotA, NodeSlot slotB)
    {
        if (slotA == null || slotB == null) return false;

        // Check if slotB is in slotA's nearbyNodes list
        return slotA.nearbyNodes.Contains(slotB);
    }

    void RemoveGlobalConnections()
    {
        List<(Node, Node)> toRemove = new List<(Node, Node)>();

        foreach (var kvp in globalConnections)
        {
            if (kvp.Key.Item1 == this || kvp.Key.Item2 == this)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.enabled = false;
                }
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var connection in toRemove)
        {
            globalConnections.Remove(connection);
        }
    }

    public void UpdateConnections(bool isNewConnection = false)
    {
        if (currentSlot == null)
        {
            HideAllConnections();
            return;
        }

        // Check each neighbor slot
        for (int i = 0; i < currentSlot.nearbyNodes.Count; i++)
        {
            NodeSlot neighborSlot = currentSlot.nearbyNodes[i];

            // If neighbor exists and has an occupying node
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                // Create a unique key for this connection (always use consistent ordering)
                var connectionKey = (this, neighborNode);
                var reverseKey = (neighborNode, this);

                // Check if this connection already exists (either direction)
                if (globalConnections.ContainsKey(connectionKey) || globalConnections.ContainsKey(reverseKey))
                {
                    continue; // Skip - connection already handled by other node
                }

                // Find an available line renderer
                LineRenderer lr = GetAvailableLineRenderer();
                if (lr != null)
                {
                    lr.enabled = true;

                    // Update line renderer settings
                    lr.widthCurve = lineWidthCurve;
                    lr.startColor = lineColor;
                    lr.endColor = lineColor;

                    if (isNewConnection && animateLines)
                    {
                        // Record start time for animation
                        lineDrawStartTime[lr] = Time.time;
                        animationCompleted[lr] = false;

                        // Start with just the start point
                        Vector3[] startPoints = new Vector3[linePoints];
                        for (int j = 0; j < linePoints; j++)
                        {
                            startPoints[j] = transform.position;
                        }
                        lr.SetPositions(startPoints);
                    }
                    else
                    {
                        // Set full line immediately
                        UpdateLinePoints(lr, transform.position, neighborNode.transform.position, 1f);
                        animationCompleted[lr] = true;
                    }

                    // Store in global connections (use consistent ordering)
                    globalConnections[connectionKey] = lr;
                }
            }
        }
    }

    LineRenderer GetAvailableLineRenderer()
    {
        foreach (var lr in connectionLines)
        {
            if (!lr.enabled)
            {
                return lr;
            }
        }
        return null;
    }

    void NotifyNeighborsToUpdateConnections()
    {
        if (currentSlot == null) return;

        foreach (var neighborSlot in currentSlot.nearbyNodes)
        {
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                neighborSlot.OccupyingNode.UpdateConnections();
            }
        }
    }

    void HideAllConnections()
    {
        foreach (var lr in connectionLines)
        {
            lr.enabled = false;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnDestroy()
    {
        // Remove this node's connections from global dictionary
        RemoveGlobalConnections();

        // Notify neighbors before destroying
        if (currentSlot != null)
        {
            // Store neighbors before clearing
            List<NodeSlot> neighbors = new List<NodeSlot>(currentSlot.GetValidNeighbors());

            // Free up slot
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;

            // Notify neighbors to update
            foreach (var neighborSlot in neighbors)
            {
                if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                {
                    neighborSlot.OccupyingNode.UpdateConnections();
                }
            }
        }

        // Clean up connection lines
        foreach (var lr in connectionLines)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }

        // Clean up preview lines
        foreach (var preview in previewLines)
        {
            if (preview != null)
                Destroy(preview.gameObject);
        }

        // Notify NodeManager
        if (NodeManager.Instance != null)
        {
            NodeManager.Instance.OnNodeStateChanged();
        }
    }

    // Public methods to change node state
    public void SetNodeState(NodeState newState)
    {
        nodeState = newState;
    }

    public void SetNodeType(NodeType newType)
    {
        nodeType = newType;
    }

    // Method to place node in inventory (sets state and optionally moves it)
    public void MoveToInventory(Vector3 inventoryPosition)
    {
        // Remove connections from global dictionary
        RemoveGlobalConnections();

        // Free up current slot if occupied
        if (currentSlot != null)
        {
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;

            // Notify neighbors to update
            NotifyNeighborsToUpdateConnections();

            currentSlot = null;
        }

        // Hide connections
        HideAllConnections();

        // Hide preview lines
        HideAllPreviews();

        // Move to inventory position
        transform.position = inventoryPosition;

        // Set state to InInventory
        nodeState = NodeState.InInventory;
    }

    // Method to refresh line renderer settings (call after changing inspector values at runtime)
    public void RefreshLineSettings()
    {
        foreach (var lr in connectionLines)
        {
            lr.widthCurve = lineWidthCurve;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.positionCount = linePoints;
        }

        // Redraw connections
        UpdateConnections();
    }
}