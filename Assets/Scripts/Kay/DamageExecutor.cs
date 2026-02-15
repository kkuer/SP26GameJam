using UnityEngine;

public class DamageExecutor : MonoBehaviour
{
    public float damageToDeal;

    public void DealDamage(HealthTracker target)
    {
        target.TakeDamage(damageToDeal);
    }
}
