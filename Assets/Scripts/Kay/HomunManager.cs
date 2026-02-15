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

    [Header("Ricochet Times")]
    public int pierceRiochetTimes;
    public int burstRiochetTimes;
    public int explodeRiochetTimes;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        UpdateActiveEmitters();
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


    public void UpdateActiveEmitters()
    {
        BuildInfo.Instance.activeAbilities.ForEach(ability =>
        {
            switch (ability.abilityType) // set abilities active if present in the build
            {
                case EffectType.AuraBurst:
                    AURA.SetActive(true);
                    break;
                case EffectType.Thorns:
                    THORNS.SetActive(true);
                    break;
                case EffectType.MeleeSwipe:
                    SWORD.SetActive(true);
                    break;
                case EffectType.PiercingShot:
                    PIERCE.SetActive(true);
                    break;
                case EffectType.BurstShot:
                    BURST.SetActive(true);
                    break;
                case EffectType.ExplosiveShot:
                    EXPLODE.SetActive(true);
                    break;
            }
        });
    }

    private void AbilityConfig(EffectType ability, WeaponDistributor toConfig) // set spread based on modifiers
    { 
    
    }
}
