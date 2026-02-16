using UnityEngine;

public class WinLoseCon : MonoBehaviour
{
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
            Debug.Log("You win!");

            if (timeLeft <= 0)
            {
                EnemySpawner.instance.ClearEnemies();
            }
        }
    }
}
