using UnityEngine;

public class DamageExecutor : MonoBehaviour
{
    public float damageToDeal;
    public EffectType damageSource;
    

    public void DealDamage(HealthTracker target)
    {
        Debug.Log("Enemy hit by a weapon");
        target.TakeDamage(damageToDeal, Color.red);
    }
}
