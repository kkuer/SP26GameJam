using UnityEngine;

public class HomunManager : MonoBehaviour
{
    public static HomunManager instance;

    public GameObject AURA;
    public GameObject THORNS;
    public GameObject SWORD;
    public GameObject PIERCE;
    public GameObject BURST;
    public GameObject EXPLODE;

    public HealthTracker healthTracker;

    [Header("Ricochet Times")]
    public int pierceRiochetTimes;
    public int burstRiochetTimes;
    public int explodeRiochetTimes;
    

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        healthTracker = GetComponent<HealthTracker>();

    }
    //public int GetRicochetTimes(EffectType abilityType)
    //{
    //    return BuildInfo.Instance.GetModifierCountForAbility(abilityType, EffectType.Ricochet);
    //}

    //public void SetRicochetTimes()
    //{
    //    pierceRiochetTimes = GetRicochetTimes(EffectType.PiercingShot);
    //    burstRiochetTimes = GetRicochetTimes(EffectType.BurstShot);
    //    explodeRiochetTimes = GetRicochetTimes(EffectType.ExplosiveShot);
    //}

    private void Start()
    {
        
        healthTracker.maxHealth *= BuildInfo.Instance.playerHealthMultiplier;
        healthTracker.SetHealth();
        UpdateActiveEmitters();
    }
    //
    public void UpdateActiveEmitters()
    {
        BuildInfo.Instance.activeAbilities.ForEach(ability =>
        {
            switch (ability.abilityType) // set abilities active if present in the build
            {
                case EffectType.AuraBurst:
                    AURA.SetActive(true);
                    AbilityConfig(ability, AURA.GetComponent<WeaponDistributor>());
                    break;
                case EffectType.Thorns:
                    THORNS.SetActive(true);
                    AbilityConfig(ability, THORNS.GetComponent<WeaponDistributor>());
                    break;
                case EffectType.MeleeSwipe:
                    SWORD.SetActive(true);
                    AbilityConfig(ability, SWORD.GetComponent<WeaponDistributor>());
                    break;
                case EffectType.PiercingShot:
                    PIERCE.SetActive(true);
                    AbilityConfig(ability, PIERCE.GetComponent<WeaponDistributor>());
                    break;
                case EffectType.BurstShot:
                    BURST.SetActive(true);
                    AbilityConfig(ability, BURST.GetComponent<WeaponDistributor>());
                    break;
                case EffectType.ExplosiveShot:
                    EXPLODE.SetActive(true);
                    AbilityConfig(ability, EXPLODE.GetComponent<WeaponDistributor>());
                    break;
            }
        });
    }

    private void AbilityConfig(ActiveAbility ability, WeaponDistributor toConfig) // set spread based on modifiers
    {
        toConfig.multishotCount = ability.multiShotCount;
        toConfig.nodeFireRate = ability.attackSpeedCount;
        toConfig.nodeDamage = ability.damageMultiplier;

        toConfig.dotCount = ability.damageOverTimeCount;
        toConfig.sipohonCount = ability.sizeCount;
        toConfig.sizeCount = ability.sizeCount;
        toConfig.ricochetCount = ability.ricochetCount;

        Debug.Log("Build Emmiters");

        toConfig.BuildEmitters();

    }



}
