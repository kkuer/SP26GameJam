using UnityEngine;
using System.Collections.Generic;

public class NodeSlot : MonoBehaviour
{
    [Header("Slot Identification")]
    [SerializeField] private string slotName; // Set this in the inspector
    public string SlotName => slotName;

    public List<NodeSlot> nearbyNodes = new List<NodeSlot>();

    public NodeSlotState state = NodeSlotState.Empty;
    public SlotType type;

    private Node occupyingNode;
    public Node OccupyingNode
    {
        get { return occupyingNode; }
        set { occupyingNode = value; }
    }

    private void Awake()
    {
        // Validate that slot name is set
        if (string.IsNullOrEmpty(slotName))
        {
            Debug.LogError($"NodeSlot on {gameObject.name} has no slot name assigned!");
        }
    }

    private void Start()
    {
        // If this is a core slot, make sure it's occupied by the core node
        if (type == SlotType.Center && occupyingNode == null)
        {
            // Find core node in scene
            Node coreNode = FindFirstObjectByType<Node>();
            if (coreNode != null && coreNode.Type == NodeType.Center)
            {
                coreNode.SetCurrentSlot(this);
            }
        }
    }

    public bool HasNeighborAt(int index)
    {
        return index < nearbyNodes.Count && nearbyNodes[index] != null;
    }

    public List<NodeSlot> GetValidNeighbors()
    {
        List<NodeSlot> valid = new List<NodeSlot>();
        foreach (var neighbor in nearbyNodes)
        {
            if (neighbor != null)
                valid.Add(neighbor);
        }
        return valid;
    }
}