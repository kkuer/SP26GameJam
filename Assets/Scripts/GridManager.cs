using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [Header("Hex Grid Settings")]
    [SerializeField] private float hexSize = 5f; // Distance from center to corner
    [SerializeField] private bool pointyTop = true; // True = pointy top, False = flat top

    [Header("Grid Boundaries")]
    [SerializeField] private float minX = -25f;
    [SerializeField] private float maxX = 25f;
    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 20f;

    [Header("Grid Visualization")]
    [SerializeField] private GameObject gridDotPrefab; // Assign a circle sprite prefab
    [SerializeField] private float dotScale = 0.3f; // Scale of the dots
    [SerializeField] private Color validDotColor = new Color(0, 1, 0, 0.3f); // Green semi-transparent
    [SerializeField] private Color occupiedDotColor = new Color(1, 0.5f, 0, 0.5f); // Orange for occupied
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 0;
    [SerializeField] private bool showGridDots = true;

    // Singleton instance
    public static GridManager Instance { get; private set; }

    // Public properties for nodes to access
    public float HexSize => hexSize;
    public float MinX => minX;
    public float MaxX => maxX;
    public float MinY => minY;
    public float MaxY => maxY;
    public bool PointyTop => pointyTop;

    // Grid data
    private List<Vector3> validHexPositions = new List<Vector3>();
    public IReadOnlyList<Vector3> ValidHexPositions => validHexPositions;

    // Node tracking
    private HashSet<Node> activeNodes = new HashSet<Node>();
    private Dictionary<Vector3, Node> occupiedPositions = new Dictionary<Vector3, Node>();

    // Grid dots pool
    private List<GameObject> gridDots = new List<GameObject>();

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CalculateValidHexPositions();

        if (showGridDots)
        {
            CreateGridVisualization();
        }
    }

    void CalculateValidHexPositions()
    {
        validHexPositions.Clear();

        // Calculate the range of axial coordinates needed to cover the boundary area
        int maxQ = Mathf.CeilToInt(maxX / hexSize) + 2;
        int maxR = Mathf.CeilToInt(maxY / hexSize) + 2;

        for (int q = -maxQ; q <= maxQ; q++)
        {
            for (int r = -maxR; r <= maxR; r++)
            {
                Vector3 worldPos = AxialToWorld(new Vector2Int(q, r));

                // Check if the entire hex fits within boundaries
                if (IsHexCompletelyInsideBounds(worldPos))
                {
                    validHexPositions.Add(worldPos);
                }
            }
        }

        Debug.Log($"GridManager: Generated {validHexPositions.Count} valid hex positions");
    }

    bool IsHexCompletelyInsideBounds(Vector3 center)
    {
        // Check all 6 corners of the hex
        for (int i = 0; i < 6; i++)
        {
            float angle = pointyTop ?
                (60f * i - 30f) * Mathf.Deg2Rad :
                (60f * i) * Mathf.Deg2Rad;

            Vector3 corner = center + new Vector3(
                Mathf.Cos(angle) * hexSize,
                Mathf.Sin(angle) * hexSize,
                0
            );

            // If any corner is outside bounds, the hex is invalid
            if (corner.x < minX || corner.x > maxX || corner.y < minY || corner.y > maxY)
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 AxialToWorld(Vector2Int axial)
    {
        Vector3 worldPos;

        if (pointyTop)
        {
            float x = hexSize * (Mathf.Sqrt(3) * axial.x + Mathf.Sqrt(3) / 2f * axial.y);
            float y = hexSize * (3f / 2f * axial.y);
            worldPos = new Vector3(x, y, 0);
        }
        else
        {
            float x = hexSize * (3f / 2f * axial.x);
            float y = hexSize * (Mathf.Sqrt(3) / 2f * axial.x + Mathf.Sqrt(3) * axial.y);
            worldPos = new Vector3(x, y, 0);
        }

        return worldPos;
    }

    public Vector3 GetClosestUnoccupiedHex(Vector3 position, Node requestingNode = null)
    {
        Vector3 closest = Vector3.zero;
        float closestDistance = float.MaxValue;

        // First, try to find the closest unoccupied position
        foreach (Vector3 hexPos in validHexPositions)
        {
            // Skip if position is occupied by another node
            if (IsPositionOccupied(hexPos, requestingNode))
                continue;

            float distance = Vector3.Distance(position, hexPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hexPos;
            }
        }

        // If we found an unoccupied position, return it
        if (closest != Vector3.zero)
        {
            return closest;
        }

        // If all positions are occupied, find the furthest occupied position? 
        // Or just return the closest occupied (shouldn't happen with proper node limits)
        Debug.LogWarning("No unoccupied hex positions found! All grid positions are occupied.");

        // Fallback: return the closest position even if occupied
        foreach (Vector3 hexPos in validHexPositions)
        {
            float distance = Vector3.Distance(position, hexPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hexPos;
            }
        }

        return closest;
    }

    public Vector3 ClampPositionToBoundaries(Vector3 position, float margin = 0f)
    {
        position.x = Mathf.Clamp(position.x, minX + margin, maxX - margin);
        position.y = Mathf.Clamp(position.y, minY + margin, maxY - margin);
        return position;
    }

    // Node registration methods
    public void RegisterNode(Node node, Vector3 position)
    {
        if (node == null) return;

        activeNodes.Add(node);

        // Find the snapped position
        Vector3 snappedPos = GetClosestUnoccupiedHex(position, node);

        // Occupy that position
        if (!occupiedPositions.ContainsKey(snappedPos))
        {
            occupiedPositions[snappedPos] = node;
        }

        UpdateGridVisualization();
    }

    public void UnregisterNode(Node node)
    {
        if (node == null) return;

        activeNodes.Remove(node);

        // Remove from occupied positions
        var keysToRemove = occupiedPositions.Where(kvp => kvp.Value == node)
                                            .Select(kvp => kvp.Key)
                                            .ToList();
        foreach (var key in keysToRemove)
        {
            occupiedPositions.Remove(key);
        }

        UpdateGridVisualization();
    }

    public void UpdateNodePosition(Node node, Vector3 oldPosition, Vector3 newPosition)
    {
        if (node == null) return;

        // Remove old position
        var oldPosKey = occupiedPositions.FirstOrDefault(x => x.Value == node).Key;
        if (oldPosKey != null && occupiedPositions.ContainsKey(oldPosKey))
        {
            occupiedPositions.Remove(oldPosKey);
        }

        // Add new position
        if (!occupiedPositions.ContainsKey(newPosition))
        {
            occupiedPositions[newPosition] = node;
        }

        UpdateGridVisualization();
    }

    public bool IsPositionOccupied(Vector3 position, Node requestingNode = null)
    {
        if (occupiedPositions.TryGetValue(position, out Node occupyingNode))
        {
            // Position is occupied by a different node
            return occupyingNode != requestingNode;
        }
        return false;
    }

    public Node GetNodeAtPosition(Vector3 position)
    {
        occupiedPositions.TryGetValue(position, out Node node);
        return node;
    }

    public List<Node> GetAllNodes()
    {
        return activeNodes.ToList();
    }

    void CreateGridVisualization()
    {
        if (gridDotPrefab == null)
        {
            Debug.LogWarning("Grid dot prefab not assigned! Grid visualization disabled.");
            return;
        }

        // Clear existing dots
        foreach (GameObject dot in gridDots)
        {
            if (dot != null) Destroy(dot);
        }
        gridDots.Clear();

        // Create dots for each valid hex position
        foreach (Vector3 pos in validHexPositions)
        {
            GameObject dot = Instantiate(gridDotPrefab, pos, Quaternion.identity, transform);
            ConfigureDotRenderer(dot, validDotColor, "GridDot");
            gridDots.Add(dot);
        }

        UpdateGridVisualization();
    }

    void UpdateGridVisualization()
    {
        if (!showGridDots || gridDots.Count == 0) return;

        for (int i = 0; i < gridDots.Count && i < validHexPositions.Count; i++)
        {
            GameObject dot = gridDots[i];
            Vector3 pos = validHexPositions[i];

            SpriteRenderer renderer = dot.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                if (occupiedPositions.ContainsKey(pos))
                {
                    renderer.color = occupiedDotColor;
                }
                else
                {
                    renderer.color = validDotColor;
                }
            }
        }
    }

    void ConfigureDotRenderer(GameObject dot, Color color, string gameObjectName)
    {
        dot.name = gameObjectName;
        dot.transform.localScale = Vector3.one * dotScale;

        SpriteRenderer renderer = dot.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }

        // Make sure dots don't block raycasts
        if (dot.TryGetComponent<Collider2D>(out Collider2D col))
        {
            col.enabled = false;
        }
    }

    // Public method to refresh grid visualization
    public void RefreshGridVisualization()
    {
        if (showGridDots && gridDotPrefab != null)
        {
            CreateGridVisualization();
        }
    }

    // Gizmo visualization in editor
    void OnDrawGizmosSelected()
    {
        // Draw boundary box
        Gizmos.color = Color.yellow;
        Vector3 boundaryCenter = new Vector3(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f,
            0
        );
        Vector3 boundarySize = new Vector3(
            maxX - minX,
            maxY - minY,
            1
        );
        Gizmos.DrawWireCube(boundaryCenter, boundarySize);
    }
}