using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryNodeButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public NodeColor nodeColor;

    public Color ownedColor;
    public Color unownedColor;

    public Color iconUnownedColor;

    public Image icon;

    private Image sprite;

    public int amount;
    public TMP_Text amountLabel;

    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnRadius = 3f; // Radius around center to spawn nodes
    [SerializeField] private float minDistanceBetweenNodes = 1.5f; // Minimum distance between spawned nodes
    [SerializeField] private Transform spawnCenter; // Center point to spawn around (defaults to Vector3.zero if null)
    [SerializeField] private int maxSpawnAttempts = 30; // Maximum attempts to find a valid spawn position

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    public GameObject nodeToInstantiate;

    void Start()
    {
        sprite = GetComponent<Image>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        // If no spawn center specified, use world origin
        if (spawnCenter == null)
        {
            spawnCenter = new GameObject("TempSpawnCenter").transform;
            spawnCenter.position = Vector3.zero;
        }
    }

    void Update()
    {
        amountLabel.text = amount.ToString();

        if (amount > 0)
        {
            sprite.color = ownedColor;
            icon.color = Color.white;
        }
        else
        {
            sprite.color = unownedColor;
            icon.color = iconUnownedColor;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );

        if (InventoryManager.Instance != null)
        {
            UpdateAmount();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void InstantiateNode()
    {
        if (amount <= 0) return;
        if (nodeToInstantiate == null)
        {
            Debug.LogError("No node prefab assigned to instantiate!");
            return;
        }

        // Find a valid spawn position
        Vector3 spawnPosition = FindValidSpawnPosition();

        // Instantiate the node
        GameObject newNode = Instantiate(nodeToInstantiate, spawnPosition, Quaternion.identity);

        // Decrease amount
        DecreaseAmount();

        Debug.Log($"Spawned node at {spawnPosition}");
    }

    private Vector3 FindValidSpawnPosition()
    {
        Vector3 center = spawnCenter != null ? spawnCenter.position : Vector3.zero;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Generate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 testPosition = new Vector3(
                center.x + randomCircle.x,
                center.y + randomCircle.y,
                center.z
            );

            // Check if position is valid (not too close to other nodes)
            if (IsPositionValid(testPosition))
            {
                return testPosition;
            }
        }

        // If no valid position found after max attempts, try one more time with a larger radius
        Debug.LogWarning("Could not find valid spawn position within radius, using fallback position");
        return GetFallbackSpawnPosition(center);
    }

    private bool IsPositionValid(Vector3 position)
    {
        // Find all nodes in the scene
        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);

        foreach (Node node in allNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < minDistanceBetweenNodes)
            {
                return false; // Too close to another node
            }
        }

        return true; // Position is valid
    }

    private Vector3 GetFallbackSpawnPosition(Vector3 center)
    {
        // Try positions in a spiral pattern as fallback
        for (float radius = spawnRadius; radius < spawnRadius * 3; radius += minDistanceBetweenNodes)
        {
            for (int angleStep = 0; angleStep < 8; angleStep++)
            {
                float angle = (angleStep / 8f) * 360f * Mathf.Deg2Rad;
                Vector3 testPosition = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius,
                    center.z
                );

                if (IsPositionValid(testPosition))
                {
                    return testPosition;
                }
            }
        }

        // Absolute fallback - just return center + random offset
        return center + (Vector3)Random.insideUnitCircle * spawnRadius;
    }

    // Optional: Visualize spawn radius in editor
    private void OnDrawGizmosSelected()
    {
        if (spawnCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnCenter.position, spawnRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero, spawnRadius);
        }
    }

    private void UpdateAmount()
    {
        if (nodeColor == NodeColor.Red)
        {
            amount = InventoryManager.Instance.redNodesOwned;
        }
        else if (nodeColor == NodeColor.Blue)
        {
            amount = InventoryManager.Instance.blueNodesOwned;
        }
        else if (nodeColor == NodeColor.Green)
        {
            amount = InventoryManager.Instance.greenNodesOwned;
        }
        else if (nodeColor == NodeColor.Purple)
        {
            amount = InventoryManager.Instance.purpleNodesOwned;
        }
        else if (nodeColor == NodeColor.Yellow)
        {
            amount = InventoryManager.Instance.yellowNodesOwned;
        }
        else if (nodeColor == NodeColor.Orange)
        {
            amount = InventoryManager.Instance.orangeNodesOwned;
        }
    }
    private void DecreaseAmount()
    {
        if (nodeColor == NodeColor.Red)
        {
            InventoryManager.Instance.redNodesOwned--;
        }
        else if (nodeColor == NodeColor.Blue)
        {
            InventoryManager.Instance.blueNodesOwned--;
        }
        else if (nodeColor == NodeColor.Green)
        {
            InventoryManager.Instance.greenNodesOwned--;
        }
        else if (nodeColor == NodeColor.Purple)
        {
            InventoryManager.Instance.purpleNodesOwned--;
        }
        else if (nodeColor == NodeColor.Yellow)
        {
            InventoryManager.Instance.yellowNodesOwned--;
        }
        else if (nodeColor == NodeColor.Orange)
        {
            InventoryManager.Instance.orangeNodesOwned--;
        }
    }
}