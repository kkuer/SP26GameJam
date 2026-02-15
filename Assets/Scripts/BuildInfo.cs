using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ActiveAbility
{
    public EffectType abilityType;           // Which ability this is
    public bool isActive;                     // Is this ability present in the build
    public float damageMultiplier;            // Calculated damage multiplier

    [Header("Connected Modifiers")]
    public int ricochetCount;
    public int attackSpeedCount;
    public int multiShotCount;
    public int sizeCount;
    public int damageOverTimeCount;
    public int siphonCount;

    // Helper methods (these are fine - just data manipulation)
    public int TotalModifiers => ricochetCount + attackSpeedCount + multiShotCount + sizeCount + damageOverTimeCount + siphonCount;
    public bool HasModifiers => TotalModifiers > 0;

    public int GetModifierCount(EffectType modifierType)
    {
        switch (modifierType)
        {
            case EffectType.Ricochet: return ricochetCount;
            case EffectType.AttackSpeed: return attackSpeedCount;
            case EffectType.MultiShot: return multiShotCount;
            case EffectType.Size: return sizeCount;
            case EffectType.DamageOverTime: return damageOverTimeCount;
            case EffectType.Siphon: return siphonCount;
            default: return 0;
        }
    }
}

[System.Serializable]
public class BuildInfo : MonoBehaviour
{
    public static BuildInfo Instance { get; private set; }

    [Header("Player Stats")]
    public float playerHealthMultiplier = 1f;
    public int playerSize = 1;

    [Header("Global Stats")]
    public int totalActiveNodes = 0;
    public int totalGlobalModifiers = 0;

    [Header("Active Abilities")]
    public List<ActiveAbility> activeAbilities = new List<ActiveAbility>();

    private void Awake()
    {
        // Simple singleton pattern - no scene dependencies
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Public method to clear all data (optional)
    public void ClearData()
    {
        playerHealthMultiplier = 1f;
        playerSize = 1;
        totalActiveNodes = 0;
        totalGlobalModifiers = 0;
        activeAbilities.Clear();
    }
}