using UnityEngine;

public class HomunManager : MonoBehaviour
{
    public static HomunManager instance;

    [Header("Ricochet Times")]
    public int pierceRiochetTimes;
    public int burstRiochetTimes;
    public int explodeRiochetTimes;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
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







}
