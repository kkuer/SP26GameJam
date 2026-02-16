using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    public string sceneName;

    public void SwapScene()
    {
        if (sceneName == "StartGame")
        {
            Destroy(InventoryManager.Instance);
        }
               UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
