using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    public string sceneName;

    public void SwapScene()
    {
               UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
