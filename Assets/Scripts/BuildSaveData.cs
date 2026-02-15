using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SavedNodeData
{
    public string nodeId;
    public NodeType nodeType;
    public EffectType effectType;
    public NodeState nodeState;
    public NodeColor nodeColor;

    // Save by slot name instead of position
    public string slotName;
    public SlotType slotType;
}

[System.Serializable]
public class BuildSaveData : MonoBehaviour
{
    private static BuildSaveData _instance;
    public static BuildSaveData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BuildSaveData>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BuildSaveData");
                    _instance = go.AddComponent<BuildSaveData>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Saved Build Data")]
    public List<SavedNodeData> savedNodes = new List<SavedNodeData>();

    [Header("Save State")]
    public bool hasSavedData = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearData()
    {
        savedNodes.Clear();
        hasSavedData = false;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}