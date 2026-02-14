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

    private Vector3 targetPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 snapTarget;

    private NodeSlot currentSlot;

    // Connection visualization
    private List<LineRenderer> connectionLines = new List<LineRenderer>();
    private Dictionary<NodeSlot, LineRenderer> activeConnections = new Dictionary<NodeSlot, LineRenderer>();

    // Public properties
    public NodeType Type => nodeType;
    public NodeState State => nodeState;
    public NodeSlot CurrentSlot => currentSlot;

    void Awake()
    {
        CreateLineRenderers();
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
            UpdateConnections();
            NotifyNeighborsToUpdateConnections();
        }
        else
        {
            Debug.LogWarning($"No available slots of type {nodeType} found for node!");
        }
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
                        // Optionally: snap back to original slot? For now just stay in place
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

                if (currentSlot != null)
                {
                    currentSlot.state = NodeSlotState.Occupied;
                    currentSlot.OccupyingNode = this;

                    // Update connections after snapping
                    UpdateConnections();
                    NotifyNeighborsToUpdateConnections();

                    // Notify NodeManager that state changed
                    if (NodeManager.Instance != null)
                    {
                        NodeManager.Instance.OnNodeStateChanged();
                    }
                }
            }
        }
    }

    void UpdateLinePoints(LineRenderer lr, Vector3 start, Vector3 end)
    {
        if (lr.positionCount < 2) return;

        Vector3[] points = new Vector3[lr.positionCount];

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)(lr.positionCount - 1);

            // Simple straight line interpolation
            // You could replace this with bezier curve logic if needed
            points[i] = Vector3.Lerp(start, end, t);
        }

        lr.SetPositions(points);
    }

    public void UpdateConnections()
    {
        if (currentSlot == null)
        {
            HideAllConnections();
            return;
        }

        HideAllConnections();

        int lineIndex = 0;

        // Check each neighbor slot
        for (int i = 0; i < currentSlot.nearbyNodes.Count && lineIndex < connectionLines.Count; i++)
        {
            NodeSlot neighborSlot = currentSlot.nearbyNodes[i];

            // If neighbor exists and has an occupying node
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                LineRenderer lr = connectionLines[lineIndex];
                lr.enabled = true;

                // Update line renderer settings (in case inspector values changed)
                lr.widthCurve = lineWidthCurve;
                lr.startColor = lineColor;
                lr.endColor = lineColor;

                // Set line points
                UpdateLinePoints(lr, transform.position, neighborSlot.OccupyingNode.transform.position);

                // Store in active connections
                activeConnections[neighborSlot] = lr;

                lineIndex++;
            }
        }
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
        activeConnections.Clear();
    }

    void LateUpdate()
    {
        // Update line positions if connections exist (for when other nodes move)
        if (activeConnections.Count > 0)
        {
            List<NodeSlot> toRemove = new List<NodeSlot>();

            foreach (var kvp in activeConnections)
            {
                NodeSlot neighborSlot = kvp.Key;
                LineRenderer lr = kvp.Value;

                if (neighborSlot != null && neighborSlot.OccupyingNode != null && neighborSlot.OccupyingNode.gameObject != null)
                {
                    // Update line points
                    UpdateLinePoints(lr, transform.position, neighborSlot.OccupyingNode.transform.position);
                }
                else
                {
                    // Connection no longer valid
                    lr.enabled = false;
                    toRemove.Add(neighborSlot);
                }
            }

            // Clean up any invalid connections
            foreach (var slot in toRemove)
            {
                activeConnections.Remove(slot);
            }
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