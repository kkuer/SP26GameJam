using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public int sceneIndex;

    public void Change()
    {
        if (BuildSaver.Instance != null)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                BuildSaver.Instance.SaveBeforeLeavingScene();
            }
        }

        SceneManager.LoadScene(sceneIndex);
    }
}
