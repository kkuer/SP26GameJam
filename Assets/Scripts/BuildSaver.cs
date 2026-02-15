using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildSaver : MonoBehaviour
{
    public static BuildSaver Instance { get; private set; }

    [Header("References")]
    [SerializeField] private NodeManager nodeManager;

    [Header("Node Prefabs")]
    [SerializeField] private GameObject redNodePrefab;
    [SerializeField] private GameObject blueNodePrefab;
    [SerializeField] private GameObject greenNodePrefab;
    [SerializeField] private GameObject purpleNodePrefab;
    [SerializeField] private GameObject yellowNodePrefab;
    [SerializeField] private GameObject orangeNodePrefab;

    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "YourSceneName"; // Set this to the scene where you want to load
    [SerializeField] private bool saveOnSceneUnload = true;
    [SerializeField] private bool loadOnTargetScene = true;

    private Dictionary<NodeColor, GameObject> prefabLookup;
    private bool isSaving = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // Build the prefab lookup dictionary
        prefabLookup = new Dictionary<NodeColor, GameObject>
        {
            { NodeColor.Red, redNodePrefab },
            { NodeColor.Blue, blueNodePrefab },
            { NodeColor.Green, greenNodePrefab },
            { NodeColor.Purple, purpleNodePrefab },
            { NodeColor.Yellow, yellowNodePrefab },
            { NodeColor.Orange, orangeNodePrefab }
        };
    }

    private void Start()
    {
        if (nodeManager == null)
            nodeManager = NodeManager.Instance;
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // Only load if this is the target scene and we have saved data
        if (loadOnTargetScene && scene.name == targetSceneName &&
            BuildSaveData.Instance != null && BuildSaveData.Instance.hasSavedData)
        {
            Debug.Log($"Target scene detected, loading build...");
            Invoke(nameof(LoadBuild), 0.1f);
        }
    }

    // Call this BEFORE leaving a scene to save
    public void SaveBeforeLeavingScene()
    {
        Debug.Log("Saving before leaving scene...");
        SaveBuild();
    }

    public void SaveBuild()
    {
        if (isSaving)
        {
            Debug.Log("Already saving, skipping...");
            return;
        }

        isSaving = true;
        Debug.Log("SaveBuild started");

        if (BuildSaveData.Instance == null)
        {
            Debug.LogError("BuildSaveData instance not found!");
            isSaving = false;
            return;
        }

        var saveData = BuildSaveData.Instance;
        saveData.ClearData();

        // Find all nodes EXCEPT the core node
        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None)
            .Where(n => n.Type != NodeType.Center)
            .ToArray();

        Debug.Log($"Found {allNodes.Length} nodes to save");

        if (allNodes.Length == 0)
        {
            Debug.Log("No nodes to save");
            isSaving = false;
            return;
        }

        Dictionary<Node, SavedNodeData> nodeToData = new Dictionary<Node, SavedNodeData>();

        // First pass: create saved data for each node
        foreach (var node in allNodes)
        {
            NodeColor nodeColor = GetNodeColor(node);

            SavedNodeData data = new SavedNodeData
            {
                nodeId = System.Guid.NewGuid().ToString(),
                nodeType = node.Type,
                effectType = node.Effect,
                nodeState = node.State,
                nodeColor = nodeColor,
                posX = node.transform.position.x,
                posY = node.transform.position.y,
                posZ = node.transform.position.z
            };

            if (node.CurrentSlot != null)
            {
                data.slotId = node.CurrentSlot.gameObject.name + "_" + node.CurrentSlot.GetInstanceID();
                data.slotType = node.CurrentSlot.type;
            }

            nodeToData[node] = data;
            saveData.savedNodes.Add(data);
        }

        // Second pass: record connections
        foreach (var node in allNodes)
        {
            if (nodeToData.ContainsKey(node) && node.CurrentSlot != null)
            {
                var nodeData = nodeToData[node];

                foreach (var neighborSlot in node.CurrentSlot.nearbyNodes)
                {
                    if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                    {
                        Node neighbor = neighborSlot.OccupyingNode;
                        if (neighbor.Type == NodeType.Center) continue;

                        if (nodeToData.ContainsKey(neighbor))
                        {
                            nodeData.connectedNodeIds.Add(nodeToData[neighbor].nodeId);
                        }
                    }
                }
            }
        }

        saveData.hasSavedData = true;
        Debug.Log($"Build saved: {saveData.savedNodes.Count} nodes");
        isSaving = false;
    }

    public void LoadBuild()
    {
        Debug.Log("LoadBuild started");

        if (BuildSaveData.Instance == null || !BuildSaveData.Instance.hasSavedData)
        {
            Debug.Log("No saved build data found.");
            return;
        }

        if (nodeManager == null)
            nodeManager = NodeManager.Instance;

        var saveData = BuildSaveData.Instance;
        Debug.Log($"Loading {saveData.savedNodes.Count} nodes");

        // Clear existing nodes (preserve core)
        ClearExistingNodes();

        // Dictionary to map saved IDs to instantiated nodes
        Dictionary<string, Node> loadedNodes = new Dictionary<string, Node>();

        // First pass: instantiate all nodes from their specific prefabs
        foreach (var savedNode in saveData.savedNodes)
        {
            // Get the correct prefab for this node's color
            if (!prefabLookup.TryGetValue(savedNode.nodeColor, out GameObject prefab) || prefab == null)
            {
                Debug.LogError($"No prefab found for node color: {savedNode.nodeColor}");
                continue;
            }

            // Instantiate from the color-specific prefab
            GameObject newNodeObj = Instantiate(prefab);
            Node newNode = newNodeObj.GetComponent<Node>();

            if (newNode == null) continue;

            // Set all saved properties
            newNode.SetNodeType(savedNode.nodeType);
            newNode.SetEffectType(savedNode.effectType);
            newNode.SetNodeState(savedNode.nodeState);

            // Set position
            newNodeObj.transform.position = new Vector3(savedNode.posX, savedNode.posY, savedNode.posZ);

            // If it was in a slot, try to place it there
            if (!string.IsNullOrEmpty(savedNode.slotId))
            {
                NodeSlot slot = FindSlotById(savedNode.slotId, savedNode.slotType);
                if (slot != null && slot.state == NodeSlotState.Empty)
                {
                    newNodeObj.transform.position = slot.transform.position;
                    newNode.SetCurrentSlot(slot);
                }
            }

            loadedNodes[savedNode.nodeId] = newNode;
        }

        // Let the nodes restore their connections automatically
        foreach (var node in loadedNodes.Values)
        {
            node.UpdateConnections(false);
        }

        // Trigger BuildManager to recalculate
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.OnNodeStateChanged();
        }

        Debug.Log($"Build loaded: {loadedNodes.Count} nodes");
    }

    private NodeColor GetNodeColor(Node node)
    {
        // You need to add NodeColor to your Node class
        // For now, let's try to determine by name
        if (node.name.Contains("Red")) return NodeColor.Red;
        if (node.name.Contains("Blue")) return NodeColor.Blue;
        if (node.name.Contains("Green")) return NodeColor.Green;
        if (node.name.Contains("Purple")) return NodeColor.Purple;
        if (node.name.Contains("Yellow")) return NodeColor.Yellow;
        if (node.name.Contains("Orange")) return NodeColor.Orange;

        // Default fallback
        return NodeColor.Red;
    }

    private void ClearExistingNodes()
    {
        Node[] existing = FindObjectsByType<Node>(FindObjectsSortMode.None);

        foreach (var node in existing)
        {
            if (node.Type == NodeType.Center)
            {
                // Reset core node's slot
                if (node.CurrentSlot != null)
                {
                    node.CurrentSlot.state = NodeSlotState.Empty;
                    node.CurrentSlot.OccupyingNode = null;
                }
                continue;
            }

            Destroy(node.gameObject);
        }
    }

    private NodeSlot FindSlotById(string slotId, SlotType type)
    {
        if (nodeManager == null) return null;

        foreach (var slot in nodeManager.allNodes)
        {
            string currentId = slot.gameObject.name + "_" + slot.GetInstanceID();
            if (currentId == slotId)
                return slot;
        }

        // Fallback: find empty slot of same type
        foreach (var slot in nodeManager.allNodes)
        {
            if (slot.type == type && slot.state == NodeSlotState.Empty)
                return slot;
        }

        return null;
    }

    // Manual methods
    public void SaveNow() => SaveBuild();
    public void LoadNow() => LoadBuild();
    public void ClearSavedData() => BuildSaveData.Instance?.ClearData();
}