using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 10f;

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 15f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualDotPrefab; // Optional: for node-specific dots
    [SerializeField] private Color dragDotColor = Color.blue;
    [SerializeField] private Color targetDotColor = Color.red;
    [SerializeField] private Color blockedTargetColor = new Color(1, 0.5f, 0, 0.8f); // Orange for blocked
    [SerializeField] private float feedbackDotScale = 0.4f;

    private Vector3 targetPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 snapTarget;

    // Visual feedback dots
    private GameObject targetDot;
    private GameObject mouseDot;

    // Track current snapped position
    private Vector3 currentSnappedPosition;

    // Unique ID for this node
    private string nodeId;

    void Start()
    {
        nodeId = System.Guid.NewGuid().ToString();

        // Create visual feedback dots if prefab is assigned
        if (visualDotPrefab != null)
        {
            CreateFeedbackDots();
        }

        // Snap to closest unoccupied position on start
        SnapToClosestUnoccupiedPosition();
    }

    void SnapToClosestUnoccupiedPosition()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("GridManager not found in scene!");
            return;
        }

        // Find the closest unoccupied hex position
        Vector3 closestUnoccupied = GridManager.Instance.GetClosestUnoccupiedHex(transform.position, this);
        currentSnappedPosition = closestUnoccupied;

        // Instantly snap to that position
        transform.position = closestUnoccupied;

        // Register with GridManager
        GridManager.Instance.RegisterNode(this, closestUnoccupied);

        Debug.Log($"Node {nodeId} snapped to unoccupied position: {closestUnoccupied}");
    }

    void CreateFeedbackDots()
    {
        // Target dot (where node will snap)
        targetDot = Instantiate(visualDotPrefab, Vector3.zero, Quaternion.identity, transform.parent);
        ConfigureFeedbackDot(targetDot, targetDotColor, "TargetDot");
        targetDot.SetActive(false);

        // Mouse dot (current drag position)
        mouseDot = Instantiate(visualDotPrefab, Vector3.zero, Quaternion.identity, transform.parent);
        ConfigureFeedbackDot(mouseDot, dragDotColor, "MouseDot");
        mouseDot.SetActive(false);
    }

    void ConfigureFeedbackDot(GameObject dot, Color color, string name)
    {
        dot.name = name;
        dot.transform.localScale = Vector3.one * feedbackDotScale;

        SpriteRenderer renderer = dot.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
            // Use the same sorting as grid, but higher order
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 1;
        }

        // Don't block raycasts
        if (dot.TryGetComponent<Collider2D>(out Collider2D col))
        {
            col.enabled = false;
        }
    }

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPos();
        targetPosition = transform.position;
        isDragging = true;
        isSnapping = false;

        // Unregister current position temporarily
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterNode(this);
        }
    }

    void OnMouseDrag()
    {
        targetPosition = GetMouseWorldPos() + offset;

        // Update mouse dot position
        if (mouseDot != null && mouseDot.activeSelf)
        {
            mouseDot.transform.position = targetPosition;
        }

        // Check for available snap positions
        if (targetDot != null && GridManager.Instance != null)
        {
            // Find the closest unoccupied hex to the mouse position
            Vector3 closestUnoccupied = GridManager.Instance.GetClosestUnoccupiedHex(targetPosition, this);
            bool isOccupied = GridManager.Instance.IsPositionOccupied(closestUnoccupied, this);

            targetDot.transform.position = closestUnoccupied;
            targetDot.SetActive(true);

            // Change color based on availability
            SpriteRenderer renderer = targetDot.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = isOccupied ? blockedTargetColor : targetDotColor;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Get snap target from GridManager (only unoccupied positions)
        if (GridManager.Instance != null)
        {
            snapTarget = GridManager.Instance.GetClosestUnoccupiedHex(transform.position, this);
        }
        else
        {
            Debug.LogError("GridManager not found in scene!");
            return;
        }

        isSnapping = true;

        // Hide mouse dot
        if (mouseDot != null)
        {
            mouseDot.SetActive(false);
        }

        // Update target dot
        if (targetDot != null)
        {
            targetDot.transform.position = snapTarget;
            SpriteRenderer renderer = targetDot.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = targetDotColor; // Should always be unoccupied now
            }
        }
    }

    void Update()
    {
        if (isDragging)
        {
            // Clamp target position to boundaries (with hex margin)
            if (GridManager.Instance != null)
            {
                targetPosition = GridManager.Instance.ClampPositionToBoundaries(
                    targetPosition,
                    GridManager.Instance.HexSize
                );
            }

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                dragSpeed * Time.deltaTime
            );

            // Update visual feedback
            if (mouseDot != null)
            {
                mouseDot.SetActive(true);
                mouseDot.transform.position = targetPosition;
            }
        }
        else if (isSnapping)
        {
            // Smoothly snap to grid
            transform.position = Vector3.Lerp(
                transform.position,
                snapTarget,
                snapSpeed * Time.deltaTime
            );

            // Update target dot
            if (targetDot != null)
            {
                targetDot.transform.position = snapTarget;
            }

            // Check if we're close enough to stop snapping
            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                transform.position = snapTarget;
                currentSnappedPosition = snapTarget;
                isSnapping = false;

                // Register final position with GridManager
                if (GridManager.Instance != null)
                {
                    // Pass old position (null) and new position
                    GridManager.Instance.UpdateNodePosition(this, Vector3.zero, snapTarget);
                }

                // Hide target dot
                if (targetDot != null)
                {
                    targetDot.SetActive(false);
                }
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
        // Unregister from GridManager
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterNode(this);
        }

        // Clean up dots
        if (targetDot != null) Destroy(targetDot);
        if (mouseDot != null) Destroy(mouseDot);
    }

    // Helper property to get hex size from GridManager
    private float hexSize
    {
        get
        {
            if (GridManager.Instance != null)
                return GridManager.Instance.HexSize;
            return 5f; // Default fallback
        }
    }
}