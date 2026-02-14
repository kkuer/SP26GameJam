using UnityEngine;
using System.Collections.Generic;

public class NodeSlot : MonoBehaviour
{
    public List<NodeSlot> nearbyNodes = new List<NodeSlot>();

    public NodeSlotState state = NodeSlotState.Empty;
    public SlotType type;

    void Start()
    {
        CreateConnections();
    }

    void Update()
    {
        UpdateLinePositions();
    }

    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.white;

    private LineRenderer lineRenderer;

    
    void CreateConnections()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        UpdateLinePositions();
    }
    
    void UpdateLinePositions()
    {
        Vector3[] positions = new Vector3[nearbyNodes.Count];
        for (int i = 0; i < nearbyNodes.Count; i++)
        {
            if (nearbyNodes[i] == null) continue;
            positions[i] = nearbyNodes[i].transform.position;
            positions[i].z = 0;
        }
        
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public void FindNearbyNode()
    {

    }
}
