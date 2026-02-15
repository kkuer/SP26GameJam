using UnityEngine;
using System.Collections.Generic;

public class NodeSlot : MonoBehaviour
{
    public List<NodeSlot> nearbyNodes = new List<NodeSlot>();

    public NodeSlotState state = NodeSlotState.Empty;
    public SlotType type;

    private Node occupyingNode;
    public Node OccupyingNode
    {
        get { return occupyingNode; }
        set { occupyingNode = value; }
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