using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
            enemy.SpawnPickup();
            return;
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
