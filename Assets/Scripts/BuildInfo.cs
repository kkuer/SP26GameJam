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

    // Helper to get total modifiers for this ability
    public int TotalModifiers => ricochetCount + attackSpeedCount + multiShotCount + sizeCount + damageOverTimeCount + siphonCount;

    // Helper to check if ability has any modifiers
    public bool HasModifiers => TotalModifiers > 0;

    // Helper to get count of specific modifier type
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
    public int playerSize = 1;  // Size is now an int, reflects total nodes

    [Header("Global Stats")]
    public int totalActiveNodes = 0;           // Total number of nodes (abilities + modifiers)
    public int totalGlobalModifiers = 0;        // Total number of modifiers only

    [Header("Active Abilities")]
    public List<ActiveAbility> activeAbilities = new List<ActiveAbility>();

    // Quick lookup by ability type
    private Dictionary<EffectType, ActiveAbility> abilityLookup = new Dictionary<EffectType, ActiveAbility>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    // Called by BuildManager to populate data
    public void SetBuildData(
        float healthMultiplier,
        int size,
        int totalNodes,
        int globalModifiers,
        List<ActiveAbility> abilities)
    {
        // Set player stats
        playerHealthMultiplier = healthMultiplier;
        playerSize = size;

        // Set global stats
        totalActiveNodes = totalNodes;
        totalGlobalModifiers = globalModifiers;

        // Set active abilities
        activeAbilities = abilities;

        // Build lookup dictionary
        abilityLookup.Clear();
        foreach (var ability in activeAbilities)
        {
            abilityLookup[ability.abilityType] = ability;
        }

        Debug.Log($"BuildInfo: {GetActiveAbilityCount()} active abilities, {totalActiveNodes} total nodes, {totalGlobalModifiers} modifiers");
    }

    // Easy access methods

    // Get number of active abilities
    public int GetActiveAbilityCount()
    {
        return activeAbilities.Count;
    }

    // Get number of modifiers only
    public int GetModifierCount()
    {
        return totalGlobalModifiers;
    }

    // Get all active ability types
    public List<EffectType> GetActiveAbilityTypes()
    {
        List<EffectType> types = new List<EffectType>();
        foreach (var ability in activeAbilities)
        {
            types.Add(ability.abilityType);
        }
        return types;
    }

    // Get specific ability data
    public ActiveAbility GetAbility(EffectType abilityType)
    {
        if (abilityLookup.ContainsKey(abilityType))
            return abilityLookup[abilityType];
        return null;
    }

    // Check if an ability is active
    public bool IsAbilityActive(EffectType abilityType)
    {
        return abilityLookup.ContainsKey(abilityType);
    }

    // Get damage multiplier for an ability
    public float GetAbilityDamage(EffectType abilityType)
    {
        var ability = GetAbility(abilityType);
        return ability != null ? ability.damageMultiplier : 1f;
    }

    // Get modifier count for a specific ability and modifier type
    public int GetModifierCountForAbility(EffectType abilityType, EffectType modifierType)
    {
        var ability = GetAbility(abilityType);
        return ability != null ? ability.GetModifierCount(modifierType) : 0;
    }

    // Get all abilities with their modifiers as a readable string
    public string GetBuildSummary()
    {
        string summary = $"Build: {GetActiveAbilityCount()} Abilities, {totalActiveNodes} Total Nodes, {totalGlobalModifiers} Modifiers\n";
        summary += $"Player Stats - Health: {playerHealthMultiplier}x, Size: {playerSize}\n";

        foreach (var ability in activeAbilities)
        {
            summary += $"- {ability.abilityType}: {ability.damageMultiplier}x damage";

            if (ability.HasModifiers)
            {
                summary += " [";
                if (ability.ricochetCount > 0) summary += $" Ricochet:{ability.ricochetCount}";
                if (ability.attackSpeedCount > 0) summary += $" AttackSpeed:{ability.attackSpeedCount}";
                if (ability.multiShotCount > 0) summary += $" MultiShot:{ability.multiShotCount}";
                if (ability.sizeCount > 0) summary += $" Size:{ability.sizeCount}";
                if (ability.damageOverTimeCount > 0) summary += $" DoT:{ability.damageOverTimeCount}";
                if (ability.siphonCount > 0) summary += $" Siphon:{ability.siphonCount}";
                summary += " ]";
            }
            summary += "\n";
        }

        return summary;
    }

    // Debug method
    public void PrintBuildInfo()
    {
        Debug.Log(GetBuildSummary());
    }
}