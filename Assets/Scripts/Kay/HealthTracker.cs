using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public void SetHealth()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage, Color damageColor)
    {

        Debug.Log("Take damage please?");

        currentHealth -= damage;

        StartCoroutine(FlashRed(damageColor));

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle death logic here (e.g., play animation, disable object, etc.)
        Debug.Log(gameObject.name + " has died.");

        EnemyLogic enemy = GetComponent<EnemyLogic>();
        if(enemy != null)
        {
            for(int i = 0; i < EnemySpawner.instance.spawnedEnemies.Count;  i++)
            {
                if (EnemySpawner.instance.spawnedEnemies[i] == this.gameObject)
                {
                    //int myIndex = EnemySpawner.instance.spawnedEnemies.IndexOf(enemyInstance);
                    EnemySpawner.instance.spawnedEnemies.RemoveAt(i);
                }
            }


            
            
            enemy.SpawnPickup();
            return;
        }
        
        var player = GetComponent<HomunManager>();
        if(player != null)
        {
            FindFirstObjectByType<WinLoseCon>().EndGame();
        }


        Destroy(gameObject);
    }

    public IEnumerator FlashRed(Color damageColor)
    {
        var sr = GetComponent<SpriteRenderer>();

        if(sr ==null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        sr.color = damageColor;

        yield return new WaitForSeconds(0.25f);

        sr.color = Color.white;
    }
}
