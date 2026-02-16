using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public bool newGameStarted = false;

    public int redNodesOwned = 0;
    public int blueNodesOwned = 0;
    public int greenNodesOwned = 0;
    public int purpleNodesOwned = 0;
    public int yellowNodesOwned = 0;
    public int orangeNodesOwned = 0;

    public int currentLevel;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if (!newGameStarted)
        {
            currentLevel = 1;

            newGameStarted = true;

            for (int i = 0; i < 2; i++)
            {
                int randomIndex = Random.Range(0, 6);

                switch (randomIndex)
                {
                    case 0: redNodesOwned++; break;
                    case 1: blueNodesOwned++; break;
                    case 2: greenNodesOwned++; break;
                    case 3: purpleNodesOwned++; break;
                    case 4: yellowNodesOwned++; break;
                    case 5: orangeNodesOwned++; break;
                }
            }
        }
    }
}
