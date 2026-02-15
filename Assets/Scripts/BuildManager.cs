using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Node Lists")]
    public List<Node> allActiveNodes = new List<Node>();
    public List<Node> activeAbilities = new List<Node>();
    public List<Node> activeModifiers = new List<Node>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        RefreshAllNodes();
    }

    public void RefreshAllNodes()
    {
        // Clear all lists
        allActiveNodes.Clear();
        activeAbilities.Clear();
        activeModifiers.Clear();

        // Find all active nodes in the scene
        allActiveNodes = FindObjectsByType<Node>(FindObjectsSortMode.None).ToList();

        // Separate by type
        foreach (var node in allActiveNodes)
        {
            if (node.Type == NodeType.Ability)
            {
                activeAbilities.Add(node);
            }
            else if (node.Type == NodeType.Modifier)
            {
                activeModifiers.Add(node);
            }
        }

        // Calculate and send data to BuildInfo
        CalculateAndSendBuildData();
    }

    private void CalculateAndSendBuildData()
    {
        if (BuildInfo.Instance == null)
        {
            Debug.LogError("BuildInfo instance not found!");
            return;
        }

        // Calculate global stats
        int totalActiveNodes = allActiveNodes.Count;
        int totalGlobalModifiers = activeModifiers.Count;

        // Calculate ability modifier counts
        Dictionary<Node, Dictionary<EffectType, int>> abilityModifierCounts = CalculateAbilityModifierCounts();

        // Calculate ability damage multipliers
        Dictionary<Node, float> abilityDamageMultipliers = CalculateAbilityDamageMultipliers(abilityModifierCounts);

        // Calculate player stats
        float playerHealthMultiplier = 1f + (totalActiveNodes * 0.1f);
        int playerSize = totalActiveNodes;

        // Build the list of active abilities with their modifier counts
        List<ActiveAbility> activeAbilityList = new List<ActiveAbility>();

        foreach (var ability in activeAbilities)
        {
            ActiveAbility activeAbility = new ActiveAbility
            {
                abilityType = ability.Effect,
                isActive = true,
                damageMultiplier = abilityDamageMultipliers.ContainsKey(ability) ? abilityDamageMultipliers[ability] : 1f
            };

            // Set per-ability modifier counts
            if (abilityModifierCounts.ContainsKey(ability))
            {
                var counts = abilityModifierCounts[ability];
                activeAbility.ricochetCount = GetCount(counts, EffectType.Ricochet);
                activeAbility.attackSpeedCount = GetCount(counts, EffectType.AttackSpeed);
                activeAbility.multiShotCount = GetCount(counts, EffectType.MultiShot);
                activeAbility.sizeCount = GetCount(counts, EffectType.Size);
                activeAbility.damageOverTimeCount = GetCount(counts, EffectType.DamageOverTime);
                activeAbility.siphonCount = GetCount(counts, EffectType.Siphon);
            }

            activeAbilityList.Add(activeAbility);
        }

        // DIRECTLY ASSIGN to BuildInfo's public fields (no method call)
        BuildInfo.Instance.playerHealthMultiplier = playerHealthMultiplier;
        BuildInfo.Instance.playerSize = playerSize;
        BuildInfo.Instance.totalActiveNodes = totalActiveNodes;
        BuildInfo.Instance.totalGlobalModifiers = totalGlobalModifiers;
        BuildInfo.Instance.activeAbilities = activeAbilityList;

        Debug.Log($"BuildManager: {activeAbilities.Count} abilities, {totalActiveNodes} total nodes, {totalGlobalModifiers} modifiers");
    }

    private int GetCount(Dictionary<EffectType, int> counts, EffectType type)
    {
        return counts.ContainsKey(type) ? counts[type] : 0;
    }

    private Dictionary<Node, Dictionary<EffectType, int>> CalculateAbilityModifierCounts()
    {
        var abilityCounts = new Dictionary<Node, Dictionary<EffectType, int>>();

        foreach (var ability in activeAbilities)
        {
            var modifierCounts = new Dictionary<EffectType, int>();

            // Initialize all effect types to 0
            foreach (EffectType effect in System.Enum.GetValues(typeof(EffectType)))
            {
                modifierCounts[effect] = 0;
            }

            // Track visited nodes to prevent loops
            HashSet<Node> visitedNodes = new HashSet<Node>();
            visitedNodes.Add(ability);

            // Start traversal from ability's neighbors
            if (ability.CurrentSlot != null)
            {
                foreach (var neighborSlot in ability.CurrentSlot.nearbyNodes)
                {
                    if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                    {
                        Node neighborNode = neighborSlot.OccupyingNode;

                        if (neighborNode.Type == NodeType.Modifier)
                        {
                            TraverseModifierTree(neighborNode, modifierCounts, visitedNodes);
                        }
                    }
                }
            }

            abilityCounts[ability] = modifierCounts;
        }

        return abilityCounts;
    }

    private void TraverseModifierTree(Node currentNode, Dictionary<EffectType, int> modifierCounts, HashSet<Node> visitedNodes)
    {
        if (currentNode == null || visitedNodes.Contains(currentNode)) return;

        visitedNodes.Add(currentNode);
        modifierCounts[currentNode.Effect]++;

        if (currentNode.CurrentSlot != null)
        {
            foreach (var neighborSlot in currentNode.CurrentSlot.nearbyNodes)
            {
                if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                {
                    Node neighborNode = neighborSlot.OccupyingNode;

                    // Don't traverse through abilities
                    if (neighborNode.Type == NodeType.Ability) continue;

                    if (neighborNode.Type == NodeType.Modifier && !visitedNodes.Contains(neighborNode))
                    {
                        TraverseModifierTree(neighborNode, modifierCounts, visitedNodes);
                    }
                }
            }
        }
    }

    private Dictionary<Node, float> CalculateAbilityDamageMultipliers(Dictionary<Node, Dictionary<EffectType, int>> abilityModifierCounts)
    {
        var multipliers = new Dictionary<Node, float>();

        foreach (var ability in activeAbilities)
        {
            float multiplier = 1f;

            if (abilityModifierCounts.ContainsKey(ability))
            {
                int totalModifiers = 0;
                foreach (var kvp in abilityModifierCounts[ability])
                {
                    totalModifiers += kvp.Value;
                }
                multiplier = 1f + (totalModifiers * 0.1f);
            }

            multipliers[ability] = multiplier;
        }

        return multipliers;
    }

    public void OnNodeStateChanged()
    {
        Invoke(nameof(RefreshAllNodes), 0.1f);
    }
}