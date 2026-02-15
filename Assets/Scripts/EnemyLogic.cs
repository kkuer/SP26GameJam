using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    public float hitDamage = 10;
    private void Start()
    {
        GetComponent<HealthTracker>().SetHealth();
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        var damageObj = col.gameObject.GetComponent<DamageExecutor>();
        if (damageObj != null)
        {
            damageObj.DealDamage(GetComponent<HealthTracker>());
        }
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        var player = col.gameObject.GetComponent<HomunManager>();
        if (player != null)
        {
            //deal damage to the player 
            player.healthTracker.TakeDamage(hitDamage);        
        }
    }
}
