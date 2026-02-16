using UnityEngine;
using TMPro;

public class LevelNumber : MonoBehaviour
{
    public TMP_Text levelText;

    private void Start()
    {
        if (InventoryManager.Instance.currentLevel <9)
        levelText.text = "TEST 0" +  (InventoryManager.Instance.currentLevel + 1);
        else
            levelText.text = "TEST " + (InventoryManager.Instance.currentLevel + 1);
    }

    void Update()
    {
            
    }
}
