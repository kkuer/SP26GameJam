using UnityEngine;

public class DamageExecutor : MonoBehaviour
{
    public float damageToDeal;
    public EffectType damageSource;
    

    public void DealDamage(HealthTracker target)
    {
        target.TakeDamage(damageToDeal);
    }
}
