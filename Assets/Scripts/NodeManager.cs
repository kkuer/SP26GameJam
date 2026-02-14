using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public List<NodeSlot> allNodes = new List<NodeSlot>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RedrawConnections();
    }

    public void RedrawConnections()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            NodeSlot node = allNodes[i];
            //node.CreateConnections();
        }
    }
}
