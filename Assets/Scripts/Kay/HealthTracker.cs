using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle death logic here (e.g., play animation, disable object, etc.)
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
}
