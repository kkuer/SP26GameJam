using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int redNodesOwned = 0;
    public int blueNodesOwned = 0;
    public int greenNodesOwned = 0;
    public int purpleNodesOwned = 0;
    public int yellowNodesOwned = 0;
    public int orangeNodesOwned = 0;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
}
