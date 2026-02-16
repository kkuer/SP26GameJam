using System.Collections;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    public float hitDamage = 10;

    public float lifestealMult = 10;
    public int baseDotDamage = 10;
    public int hitSoundIndex = 3;

    private void Start()
    {
        GetComponent<HealthTracker>().SetHealth();
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Enemy Hit");
        var damageObj = col.gameObject.GetComponent<DamageExecutor>();


        if (damageObj != null)
        {
            Debug.Log("Enemy Hit 2");
            AudioManager.Instance.PlaySound(hitSoundIndex);
            damageObj.DealDamage(GetComponent<HealthTracker>());

            switch (damageObj.damageSource)
            {
                case EffectType.PiercingShot:
                    DoTApplied(HomunManager.instance.PIERCE.GetComponent<WeaponDistributor>());
                    break;

                case EffectType.BurstShot:
                    DoTApplied(HomunManager.instance.BURST.GetComponent<WeaponDistributor>());
                    break;

                case EffectType.ExplosiveShot:
                    DoTApplied(HomunManager.instance.EXPLODE.GetComponent<WeaponDistributor>());
                    break;

                case EffectType.MeleeSwipe:
                    Debug.Log("Enemy Hit 3");
                    DoTApplied(HomunManager.instance.SWORD.GetComponent<WeaponDistributor>());
                    SiphonHealth(HomunManager.instance.PIERCE.GetComponent<WeaponDistributor>());
                    break;

                case EffectType.AuraBurst:
                    DoTApplied(HomunManager.instance.AURA.GetComponent<WeaponDistributor>());
                    SiphonHealth(HomunManager.instance.AURA.GetComponent<WeaponDistributor>());
                    break;

                case EffectType.Thorns:
                    DoTApplied(HomunManager.instance.THORNS.GetComponent<WeaponDistributor>());
                    SiphonHealth(HomunManager.instance.THORNS.GetComponent<WeaponDistributor>());
                    break;
            }
        }
    }
    public void SiphonHealth(WeaponDistributor weaponLogic)
    {
        HomunManager.instance.gameObject.GetComponent<HealthTracker>().TakeDamage(-1 * weaponLogic.sipohonCount * lifestealMult, Color.green);
    }
    public void DoTApplied(WeaponDistributor weaponLogic)
    {
        var myHealth = GetComponent<HealthTracker>();
        if (myHealth == null) return;

        var dotNum = weaponLogic.dotCount;
        if(dotNum == 0) return;



        StartCoroutine(TakeDoTDamage(weaponLogic, dotNum));
    }
    public IEnumerator TakeDoTDamage(WeaponDistributor weaponLogic, int dotNum)
    {
        yield return new WaitForSeconds(1);
        GetComponent<HealthTracker>().TakeDamage(baseDotDamage * dotNum, Color.red);

        yield return new WaitForSeconds(1);
        GetComponent<HealthTracker>().TakeDamage(baseDotDamage * dotNum, Color.red);

        yield return new WaitForSeconds(1);
        GetComponent<HealthTracker>().TakeDamage(baseDotDamage * dotNum, Color.red);

    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        var player = col.gameObject.GetComponent<HomunManager>();
        if (player != null)
        {
            //deal damage to the player 
            player.healthTracker.TakeDamage(hitDamage, Color.red);        
        }
    }
}
