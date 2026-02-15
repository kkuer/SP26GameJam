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
    [SerializeField] private string targetSceneName = "NodeScene";
    [SerializeField] private bool saveOnSceneUnload = true;
    [SerializeField] private bool loadOnTargetScene = true;

    private Dictionary<NodeColor, GameObject> prefabLookup;
    private bool isSaving = false;

    // Lookup for slots by their name
    private Dictionary<string, NodeSlot> slotNameLookup;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

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
        if (loadOnTargetScene && scene.name == targetSceneName)
        {
            if (BuildSaveData.Instance != null && BuildSaveData.Instance.hasSavedData)
            {
                Debug.Log($"Scene loaded, loading build immediately...");
                LoadBuild();
            }
        }
    }

    public void SaveBeforeLeavingScene()
    {
        Debug.Log("Saving before leaving scene...");
        SaveBuild();
    }

    public void SaveBuild()
    {
        if (isSaving) return;

        isSaving = true;

        if (BuildSaveData.Instance == null)
        {
            Debug.LogError("BuildSaveData instance not found!");
            isSaving = false;
            return;
        }

        var saveData = BuildSaveData.Instance;
        saveData.ClearData();

        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None)
            .Where(n => n.Type != NodeType.Center)
            .ToArray();

        foreach (var node in allNodes)
        {
            if (string.IsNullOrEmpty(node.CurrentSlotName))
            {
                Debug.LogWarning($"Node {node.Effect} has no slot name, skipping save");
                continue;
            }

            SavedNodeData data = new SavedNodeData
            {
                nodeId = System.Guid.NewGuid().ToString(),
                nodeType = node.Type,
                effectType = node.Effect,
                nodeState = node.State,
                nodeColor = node.NodeColor,
                slotName = node.CurrentSlotName,
                slotType = node.CurrentSlot.type
            };

            saveData.savedNodes.Add(data);
            Debug.Log($"Saved node {node.Effect} to slot {node.CurrentSlotName}");
        }

        saveData.hasSavedData = saveData.savedNodes.Count > 0;
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

        if (nodeManager == null)
        {
            Debug.LogError("NodeManager not found!");
            return;
        }

        // Build slot lookup by name
        BuildSlotLookup();

        var saveData = BuildSaveData.Instance;

        // Clear existing nodes
        ClearExistingNodes();

        // Track placed nodes
        List<Node> placedNodes = new List<Node>();

        // Place each saved node in its named slot
        foreach (var savedNode in saveData.savedNodes)
        {
            if (string.IsNullOrEmpty(savedNode.slotName))
            {
                Debug.LogWarning($"Saved node {savedNode.effectType} has no slot name, skipping");
                continue;
            }

            if (!slotNameLookup.TryGetValue(savedNode.slotName, out NodeSlot targetSlot))
            {
                Debug.LogError($"Could not find slot with name: {savedNode.slotName}");
                continue;
            }

            if (targetSlot.state != NodeSlotState.Empty)
            {
                Debug.LogError($"Slot {savedNode.slotName} is already occupied!");
                continue;
            }

            Node placedNode = PlaceNodeInSlot(savedNode, targetSlot);
            if (placedNode != null)
            {
                placedNodes.Add(placedNode);
            }
        }

        // Let connections establish
        StartCoroutine(RefreshConnections(placedNodes));

        Debug.Log($"Build loaded: {placedNodes.Count} nodes");
    }

    private void BuildSlotLookup()
    {
        slotNameLookup = new Dictionary<string, NodeSlot>();

        if (nodeManager == null)
            nodeManager = NodeManager.Instance;

        if (nodeManager != null)
        {
            Debug.Log($"Building slot lookup with {nodeManager.allNodes.Count} slots");

            foreach (var slot in nodeManager.allNodes)
            {
                // Get the slot name from the NodeSlot component
                NodeSlot nodeSlot = slot.GetComponent<NodeSlot>();
                if (nodeSlot != null && !string.IsNullOrEmpty(nodeSlot.SlotName))
                {
                    slotNameLookup[nodeSlot.SlotName] = nodeSlot;
                    Debug.Log($"Slot registered: {nodeSlot.SlotName}");
                }
                else
                {
                    Debug.LogWarning($"Slot on {slot.gameObject.name} has no name assigned!");
                }
            }
        }
    }

    private Node PlaceNodeInSlot(SavedNodeData savedNode, NodeSlot targetSlot)
    {
        if (!prefabLookup.TryGetValue(savedNode.nodeColor, out GameObject prefab) || prefab == null)
        {
            Debug.LogError($"No prefab for color: {savedNode.nodeColor}");
            return null;
        }

        Debug.Log($"Instantiating {savedNode.effectType} in slot {targetSlot.SlotName}");

        GameObject newNodeObj = Instantiate(prefab);
        Node newNode = newNodeObj.GetComponent<Node>();

        if (newNode == null) return null;

        // CRITICAL: Mark as loaded BEFORE anything else
        newNode.MarkAsLoadedFromSave();

        // Set properties
        newNode.SetNodeType(savedNode.nodeType);
        newNode.SetEffectType(savedNode.effectType);

        // Place in slot - this will set the slot and name
        newNode.SetCurrentSlot(targetSlot);

        // Set state
        newNode.SetNodeState(NodeState.Locked);

        return newNode;
    }

    private System.Collections.IEnumerator RefreshConnections(List<Node> loadedNodes)
    {
        // Wait a bit for all nodes to fully initialize
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Refreshing connections...");

        // First, clear any existing connections
        foreach (var node in loadedNodes)
        {
            // Force hide all connections first
            var method = typeof(Node).GetMethod("HideAllConnections",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(node, null);
        }

        yield return new WaitForSeconds(0.1f);

        // Now trigger new connections with animation
        foreach (var node in loadedNodes)
        {
            node.UpdateConnections(true);
        }

        yield return new WaitForSeconds(0.2f);

        // Force NodeManager to refresh
        if (NodeManager.Instance != null)
        {
            NodeManager.Instance.RefreshAllConnections();
        }

        // Update BuildManager
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.OnNodeStateChanged();
        }

        Debug.Log("Connections refreshed");
    }

    private void ClearExistingNodes()
    {
        Node[] existing = FindObjectsByType<Node>(FindObjectsSortMode.None);

        foreach (var node in existing)
        {
            if (node.Type == NodeType.Center) continue;
            Destroy(node.gameObject);
        }

        // Reset all slots to empty
        if (nodeManager != null)
        {
            foreach (var slot in nodeManager.allNodes)
            {
                slot.state = NodeSlotState.Empty;
                slot.OccupyingNode = null;
            }
        }
    }

    // Manual methods
    public void SaveNow() => SaveBuild();
    public void LoadNow() => LoadBuild();
    public void ClearSavedData() => BuildSaveData.Instance?.ClearData();
}