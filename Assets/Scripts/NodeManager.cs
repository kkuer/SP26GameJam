using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager Instance { get; private set; }

    public List<NodeSlot> allNodes = new List<NodeSlot>();
    
    [Header("Node Prefab")]
    [SerializeField] private GameObject nodePrefab;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Initial redraw of connections
        RefreshAllConnections();
    }

    // Call this to instantiate a new node
    public GameObject InstantiateNode(Vector3 position, NodeType nodeType)
    {
        if (nodePrefab == null)
        {
            Debug.LogError("Node prefab not assigned in NodeManager!");
            return null;
        }

        GameObject newNode = Instantiate(nodePrefab, position, Quaternion.identity);
        Node nodeComponent = newNode.GetComponent<Node>();
        
        // Set the node type (if the prefab has a default, this overrides it)
        // Note: You might want to set this in the prefab instead
        nodeComponent.SetNodeType(nodeType);
        
        return newNode;
    }

    // Call this to instantiate a node and snap it to the closest available slot of matching type
    public GameObject InstantiateNodeAtClosestSlot(Vector3 position, NodeType nodeType)
    {
        if (nodePrefab == null)
        {
            Debug.LogError("Node prefab not assigned in NodeManager!");
            return null;
        }

        // Find closest available slot of matching type
        NodeSlot closestSlot = GetClosestAvailableSlotOfType(position, nodeType);
        
        if (closestSlot != null)
        {
            // Instantiate at slot position
            GameObject newNode = Instantiate(nodePrefab, closestSlot.transform.position, Quaternion.identity);
            Node nodeComponent = newNode.GetComponent<Node>();
            nodeComponent.SetNodeType(nodeType);
            return newNode;
        }
        else
        {
            Debug.LogWarning($"No available slots of type {nodeType} to place node!");
            // Fallback: instantiate at provided position
            GameObject newNode = Instantiate(nodePrefab, position, Quaternion.identity);
            Node nodeComponent = newNode.GetComponent<Node>();
            nodeComponent.SetNodeType(nodeType);
            return newNode;
        }
    }

    public NodeSlot GetClosestAvailableSlotOfType(Vector3 position, NodeType nodeType)
    {
        NodeSlot closestSlot = null;
        float closestDistance = float.MaxValue;

        foreach (NodeSlot slot in allNodes)
        {
            // Check if slot is empty AND matches the node type
            if (slot.state != NodeSlotState.Occupied && slot.type == (SlotType)nodeType)
            {
                float distance = Vector3.Distance(position, slot.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }
        }

        return closestSlot;
    }

    // Get all available slots of a specific type
    public List<NodeSlot> GetAllAvailableSlotsOfType(NodeType nodeType)
    {
        List<NodeSlot> availableSlots = new List<NodeSlot>();
        
        foreach (NodeSlot slot in allNodes)
        {
            if (slot.state != NodeSlotState.Occupied && slot.type == (SlotType)nodeType)
            {
                availableSlots.Add(slot);
            }
        }
        
        return availableSlots;
    }

    public void RedrawConnections()
    {
        RefreshAllConnections();
    }

    public void RefreshAllConnections()
    {
        foreach (var slot in allNodes)
        {
            if (slot.OccupyingNode != null)
            {
                slot.OccupyingNode.UpdateConnections();
            }
        }
    }

    // Called when a node is destroyed or moved
    public void OnNodeStateChanged()
    {
        // Small delay to ensure everything is updated
        Invoke(nameof(RefreshAllConnections), 0.1f);
    }
}