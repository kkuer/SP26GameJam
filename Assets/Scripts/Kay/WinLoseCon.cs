using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class WinLoseCon : MonoBehaviour
{
    public UnityEvent onWin;
    public UnityEvent onLose;

    public float timeToWin = 60f;
    public float timeLeft;

    public TMP_Text timerText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeLeft = timeToWin;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeLeft / 60F);
        int seconds = Mathf.FloorToInt(timeLeft - minutes * 60);

        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        timerText.text = niceTime;

        if (EnemySpawner.instance.spawnedEnemies.Count == 0 || timeLeft <= 0)
        {
            Debug.Log("You win! Next level");

            if (timeLeft <= 0)
            {
                EnemySpawner.instance.ClearEnemies();
            }

            onWin?.Invoke();
        }

        if (HomunManager.instance.GetComponent<HealthTracker>() != null)
        {
            if (HomunManager.instance.GetComponent<HealthTracker>().currentHealth <= 0)
            {
                Time.timeScale = 0f;
                onLose?.Invoke();
            }
        }
    }

    public void LoseRestart()
    {
            Destroy(InventoryManager.Instance.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
