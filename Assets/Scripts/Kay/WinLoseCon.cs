using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class WinLoseCon : MonoBehaviour
{
    public UnityEvent onWin;
    public UnityEvent onLose;

    public float timeToWin = 60f;
    public float timeLeft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeLeft = timeToWin;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;

        if (EnemySpawner.instance.spawnedEnemies.Count == 0 || timeLeft <= 0)
        {
            Debug.Log("You win! Next level");

            if (timeLeft <= 0)
            {
                EnemySpawner.instance.ClearEnemies();
            }

            onWin?.Invoke();
        }
        if (HomunManager.instance.GetComponent<HealthTracker>().currentHealth <= 0)
        {
            Time.timeScale = 0f;
            onLose?.Invoke();
        }
    }

    public void LoseRestart()
    {
            Destroy(InventoryManager.Instance.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
