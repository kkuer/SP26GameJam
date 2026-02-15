using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public int sceneIndex;
    public string targetSceneName; // Optional: for name-based checking

    public void Change()
    {
        // Save before leaving if we're in the node tree scene
        // You can check by build index or scene name
        if (SceneManager.GetActiveScene().name == "NodeScene" ||
            SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (BuildSaver.Instance != null)
            {
                BuildSaver.Instance.SaveBeforeLeavingScene();
            }
        }

        SceneManager.LoadScene(sceneIndex);
    }
}