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

    [Header("Player Stats")]
    public float playerHealthMultiplier = 1f;
    public float playerSize = 1f;

    [Header("Ability Modifier Counts")]
    public Dictionary<Node, Dictionary<EffectType, int>> abilityModifierCounts = new Dictionary<Node, Dictionary<EffectType, int>>();

    [Header("Ability Damage Multipliers")]
    public Dictionary<Node, float> abilityDamageMultipliers = new Dictionary<Node, float>();

    [Header("Global Modifier Counts")]
    public Dictionary<EffectType, int> globalModifierCounts = new Dictionary<EffectType, int>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Optional: Initial scan
        RefreshAllNodes();
    }

    public void RefreshAllNodes()
    {
        // Clear all lists and dictionaries
        allActiveNodes.Clear();
        activeAbilities.Clear();
        activeModifiers.Clear();
        abilityModifierCounts.Clear();
        abilityDamageMultipliers.Clear();
        globalModifierCounts.Clear();

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

        // Calculate global modifier counts
        CalculateGlobalModifierCounts();

        // Calculate ability modifier connections
        CalculateAbilityModifierConnections();

        // Calculate ability damage multipliers
        CalculateAbilityDamageMultipliers();

        // Calculate player stats based on all nodes
        CalculatePlayerStats();

        // Debug output
        Debug.Log($"BuildManager: Found {allActiveNodes.Count} total nodes ({activeAbilities.Count} abilities, {activeModifiers.Count} modifiers)");
    }

    private void CalculateGlobalModifierCounts()
    {
        // Initialize all effect types to 0
        foreach (EffectType effect in System.Enum.GetValues(typeof(EffectType)))
        {
            globalModifierCounts[effect] = 0;
        }

        // Count each modifier by its effect type
        foreach (var modifier in activeModifiers)
        {
            if (globalModifierCounts.ContainsKey(modifier.Effect))
            {
                globalModifierCounts[modifier.Effect]++;
            }
            else
            {
                globalModifierCounts[modifier.Effect] = 1;
            }
        }
    }

    private void CalculateAbilityModifierConnections()
    {
        // Clear previous counts
        abilityModifierCounts.Clear();

        // For each ability, traverse its modifier tree
        foreach (var ability in activeAbilities)
        {
            var modifierCounts = new Dictionary<EffectType, int>();

            // Initialize all effect types to 0 for this ability
            foreach (EffectType effect in System.Enum.GetValues(typeof(EffectType)))
            {
                modifierCounts[effect] = 0;
            }

            // Track visited nodes to prevent infinite loops
            HashSet<Node> visitedNodes = new HashSet<Node>();

            // Track the starting ability
            visitedNodes.Add(ability);

            // Start traversal from this ability's neighbors
            if (ability.CurrentSlot != null)
            {
                foreach (var neighborSlot in ability.CurrentSlot.nearbyNodes)
                {
                    if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                    {
                        Node neighborNode = neighborSlot.OccupyingNode;

                        // Only start traversal from modifiers connected directly to ability
                        if (neighborNode.Type == NodeType.Modifier)
                        {
                            TraverseModifierTree(neighborNode, modifierCounts, visitedNodes, ability);
                        }
                    }
                }
            }

            // Store the counts for this ability
            abilityModifierCounts[ability] = modifierCounts;
        }
    }

    private void CalculateAbilityDamageMultipliers()
    {
        // Clear previous multipliers
        abilityDamageMultipliers.Clear();

        // Calculate multiplier for each ability based on connected modifiers
        foreach (var ability in activeAbilities)
        {
            if (abilityModifierCounts.ContainsKey(ability))
            {
                // Get total number of connected modifiers for this ability
                int totalConnectedModifiers = 0;

                foreach (var kvp in abilityModifierCounts[ability])
                {
                    totalConnectedModifiers += kvp.Value;
                }

                // Calculate multiplier: base 1.0 + (connected modifiers * 0.1)
                // Example: 3 connected nodes = 1.3 multiplier
                float multiplier = 1f + (totalConnectedModifiers * 0.1f);

                // You can also make specific modifier types affect damage differently
                // For example, if you want AttackSpeed to give more damage:
                // if (abilityModifierCounts[ability].ContainsKey(EffectType.AttackSpeed))
                // {
                //     multiplier += abilityModifierCounts[ability][EffectType.AttackSpeed] * 0.05f;
                // }

                abilityDamageMultipliers[ability] = multiplier;

                Debug.Log($"Ability {ability.Effect} has {totalConnectedModifiers} connected modifiers, damage multiplier: {multiplier}x");
            }
            else
            {
                // No connected modifiers
                abilityDamageMultipliers[ability] = 1f;
            }
        }
    }

    private void TraverseModifierTree(Node currentNode, Dictionary<EffectType, int> modifierCounts, HashSet<Node> visitedNodes, Node startingAbility)
    {
        if (currentNode == null) return;

        // Prevent infinite loops
        if (visitedNodes.Contains(currentNode)) return;

        // Add current node to visited set
        visitedNodes.Add(currentNode);

        // Count this modifier
        if (modifierCounts.ContainsKey(currentNode.Effect))
        {
            modifierCounts[currentNode.Effect]++;
        }
        else
        {
            modifierCounts[currentNode.Effect] = 1;
        }

        // Get all connected nodes through the slot system
        if (currentNode.CurrentSlot != null)
        {
            foreach (var neighborSlot in currentNode.CurrentSlot.nearbyNodes)
            {
                if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                {
                    Node neighborNode = neighborSlot.OccupyingNode;

                    // Don't traverse back to abilities (this prevents crossing through abilities)
                    if (neighborNode.Type == NodeType.Ability)
                    {
                        // Skip abilities - don't traverse through them
                        continue;
                    }

                    // Only traverse to modifier nodes
                    if (neighborNode.Type == NodeType.Modifier && !visitedNodes.Contains(neighborNode))
                    {
                        TraverseModifierTree(neighborNode, modifierCounts, visitedNodes, startingAbility);
                    }
                }
            }
        }
    }

    // Alternative method that builds a connection graph for more complex scenarios
    public Dictionary<Node, List<Node>> BuildModifierConnectionGraph()
    {
        var graph = new Dictionary<Node, List<Node>>();

        // Initialize graph for all modifiers
        foreach (var modifier in activeModifiers)
        {
            graph[modifier] = new List<Node>();
        }

        // Build connections between modifiers
        foreach (var modifier in activeModifiers)
        {
            if (modifier.CurrentSlot != null)
            {
                foreach (var neighborSlot in modifier.CurrentSlot.nearbyNodes)
                {
                    if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                    {
                        Node neighborNode = neighborSlot.OccupyingNode;

                        // Connect modifier to other modifiers
                        if (neighborNode.Type == NodeType.Modifier)
                        {
                            if (!graph[modifier].Contains(neighborNode))
                            {
                                graph[modifier].Add(neighborNode);
                            }
                        }
                    }
                }
            }
        }

        return graph;
    }

    // Method to find all modifiers connected to a specific ability (including through modifier chains)
    public HashSet<Node> GetAllModifiersForAbility(Node ability)
    {
        HashSet<Node> result = new HashSet<Node>();

        if (ability.CurrentSlot == null) return result;

        // Find modifiers directly connected to ability
        foreach (var neighborSlot in ability.CurrentSlot.nearbyNodes)
        {
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                if (neighborNode.Type == NodeType.Modifier)
                {
                    // Use BFS/DFS to find all modifiers in this chain
                    HashSet<Node> visited = new HashSet<Node>();
                    Queue<Node> queue = new Queue<Node>();

                    queue.Enqueue(neighborNode);
                    visited.Add(ability); // Don't traverse back through ability

                    while (queue.Count > 0)
                    {
                        Node current = queue.Dequeue();

                        if (visited.Contains(current)) continue;
                        visited.Add(current);

                        if (current.Type == NodeType.Modifier)
                        {
                            result.Add(current);
                        }

                        // Explore neighbors
                        if (current.CurrentSlot != null)
                        {
                            foreach (var slot in current.CurrentSlot.nearbyNodes)
                            {
                                if (slot != null && slot.OccupyingNode != null)
                                {
                                    Node nextNode = slot.OccupyingNode;

                                    // Don't traverse through abilities
                                    if (nextNode.Type != NodeType.Ability && !visited.Contains(nextNode))
                                    {
                                        queue.Enqueue(nextNode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private void CalculatePlayerStats()
    {
        // Reset base stats
        playerHealthMultiplier = 1f;
        playerSize = 1f;

        // Calculate from all active nodes (count of nodes affects health)
        playerHealthMultiplier = 1f + (allActiveNodes.Count * 0.1f); // Example formula: +10% per node

        // Calculate from global modifiers
        if (globalModifierCounts.ContainsKey(EffectType.Size))
        {
            playerSize = 1f + (globalModifierCounts[EffectType.Size] * 0.2f); // Example: +20% per Size modifier
        }

        Debug.Log($"BuildManager: Player Stats - Health Multiplier: {playerHealthMultiplier}, Size: {playerSize}");
    }

    // Public method to get modifier counts for a specific ability
    public Dictionary<EffectType, int> GetModifierCountsForAbility(Node ability)
    {
        if (abilityModifierCounts.ContainsKey(ability))
        {
            return new Dictionary<EffectType, int>(abilityModifierCounts[ability]);
        }
        return null;
    }

    // Public method to get damage multiplier for a specific ability
    public float GetAbilityDamageMultiplier(Node ability)
    {
        if (abilityDamageMultipliers.ContainsKey(ability))
        {
            return abilityDamageMultipliers[ability];
        }
        return 1f;
    }

    // Public method to get damage multiplier for a specific ability by its EffectType
    public float GetAbilityDamageMultiplierByType(EffectType abilityType)
    {
        // Find the ability node with this effect type
        Node abilityNode = activeAbilities.FirstOrDefault(a => a.Effect == abilityType);

        if (abilityNode != null && abilityDamageMultipliers.ContainsKey(abilityNode))
        {
            return abilityDamageMultipliers[abilityNode];
        }
        return 1f;
    }

    // Public method to get total count of a specific modifier globally
    public int GetGlobalModifierCount(EffectType modifierType)
    {
        if (globalModifierCounts.ContainsKey(modifierType))
        {
            return globalModifierCounts[modifierType];
        }
        return 0;
    }

    // Public method to get total count of a specific modifier connected to an ability
    public int GetModifierCountForAbility(Node ability, EffectType modifierType)
    {
        if (abilityModifierCounts.ContainsKey(ability) && abilityModifierCounts[ability].ContainsKey(modifierType))
        {
            return abilityModifierCounts[ability][modifierType];
        }
        return 0;
    }

    // Call this when nodes are added/removed/moved
    public void OnNodeStateChanged()
    {
        // Small delay to ensure everything is settled
        Invoke(nameof(RefreshAllNodes), 0.1f);
    }

    // Debug method to print all modifier connections
    public void PrintAllModifierConnections()
    {
        Debug.Log("=== ABILITY MODIFIER CONNECTIONS ===");
        foreach (var ability in activeAbilities)
        {
            Debug.Log($"Ability: {ability.Effect}");
            if (abilityModifierCounts.ContainsKey(ability))
            {
                foreach (var kvp in abilityModifierCounts[ability])
                {
                    if (kvp.Value > 0)
                    {
                        Debug.Log($"  - {kvp.Key}: {kvp.Value}");
                    }
                }
            }
        }

        Debug.Log("=== GLOBAL MODIFIER COUNTS ===");
        foreach (var kvp in globalModifierCounts)
        {
            if (kvp.Value > 0)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }

    // Debug method to print ability damage multipliers
    public void PrintAbilityDamageMultipliers()
    {
        Debug.Log("=== ABILITY DAMAGE MULTIPLIERS ===");
        foreach (var ability in activeAbilities)
        {
            float multiplier = GetAbilityDamageMultiplier(ability);
            Debug.Log($"Ability {ability.Effect}: {multiplier}x damage");
        }
    }

    // Debug method to print the modifier connection graph
    public void PrintModifierGraph()
    {
        var graph = BuildModifierConnectionGraph();

        Debug.Log("=== MODIFIER CONNECTION GRAPH ===");
        foreach (var kvp in graph)
        {
            string connections = string.Join(", ", kvp.Value.Select(m => m.Effect.ToString()));
            Debug.Log($"Modifier {kvp.Key.Effect} connects to: {connections}");
        }
    }
}