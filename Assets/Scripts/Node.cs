using UnityEngine;
using UnityEngine.UI; // Add this for UI Image
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Node : MonoBehaviour
{
    [Header("Node Settings")]
    [SerializeField] private NodeType nodeType = NodeType.Ability;
    [SerializeField] private NodeState nodeState = NodeState.Draggable;
    [SerializeField] private EffectType effectType = EffectType.PiercingShot; // Current effect

    [Header("Visual Properties")]
    [SerializeField] private NodeColor nodeColor; // Set this in each prefab

    // Add this field to Node.cs in the header section
    [Header("Slot Reference")]
    [SerializeField] private string currentSlotName; // Will be set when placed in a slot

    // Add this property
    public string CurrentSlotName => currentSlotName;

    private bool isLoadedFromSave = false;

    // HOVER PREVIEW VARIABLES - Add these with the other private variables
    private bool isHoveringOverSlot = false;
    private NodeSlot hoveredSlot;
    private EffectType originalEffectType;
    private Sprite originalIcon;
    private string originalText;
    private Color originalIconColor;
    private Color originalTextColor;
    private NodeType originalNodeType;

    public NodeColor NodeColor => nodeColor;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text effectText; // Reference to the TextMeshPro component
    [SerializeField] private Image iconImage; // Reference to the UI Image component
    [SerializeField] private bool updateUIOnChange = true; // Toggle UI updates

    [Header("Camera Threshold Settings")]
    [SerializeField] private float cameraThreshold = 13f; // Threshold for switching between text and icon
    [SerializeField] private bool showIconAboveThreshold = true; // If true, show icon when ortho size > threshold, text when < threshold
    [SerializeField] private float checkInterval = 0.2f; // How often to check camera size (performance)

    [Header("Icon Sprites")]
    [SerializeField] private Sprite piercingShotIcon;
    [SerializeField] private Sprite burstShotIcon;
    [SerializeField] private Sprite explosiveShotIcon;
    [SerializeField] private Sprite meleeSwipeIcon;
    [SerializeField] private Sprite auraBurstIcon;
    [SerializeField] private Sprite thornsIcon;
    [SerializeField] private Sprite ricochetIcon;
    [SerializeField] private Sprite attackSpeedIcon;
    [SerializeField] private Sprite multiShotIcon;
    [SerializeField] private Sprite sizeIcon;
    [SerializeField] private Sprite damageOverTimeIcon;
    [SerializeField] private Sprite siphonIcon;

    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 10f;

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 15f;

    [Header("Line Renderer Settings")]
    [SerializeField] private int linePoints = 8; // Number of points per line (including start and end)
    [SerializeField] private AnimationCurve lineWidthCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.1f);
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;

    [Header("Preview Line Settings")]
    [SerializeField] private bool showPreviewWhileDragging = true;
    [SerializeField] private Color previewLineColor = new Color(1f, 1f, 0f, 0.5f); // Yellow semi-transparent
    [SerializeField] private Color conversionPreviewColor = new Color(1f, 0f, 1f, 0.5f); // Magenta for conversion preview
    [SerializeField] private float previewLineWidth = 0.15f;
    [SerializeField] private int maxPreviewLines = 6; // Maximum number of preview lines
    [SerializeField] private Material previewMaterial; // Optional: different material for preview

    [Header("Preview Node Feedback")]
    [SerializeField] private GameObject previewNodePrefab; // Prefab to instantiate on target slot
    [SerializeField] private float previewNodeScale = 1.2f; // Scale multiplier for preview node
    [SerializeField] private float previewNodePulseSpeed = 2f; // Speed of pulse animation
    [SerializeField] private float previewNodePulseAmount = 0.2f; // Amount to pulse (0 = no pulse)
    [SerializeField] private float previewNodeZoomSpeed = 1.5f; // Speed of zoom in/out
    [SerializeField] private float previewNodeZoomAmount = 0.15f; // Amount to zoom (0 = no zoom)

    [Header("Preview Pulse Settings")]
    [SerializeField] private float pulseSpeed = 5f; // Speed of the pulse effect
    [SerializeField] private float pulseIntensity = 0.2f; // How much the brightness varies (0 = no pulse, 1 = full variation)
    [SerializeField] private float pulseBaseAlpha = 0.5f; // Base alpha value (same as previewLineColor.a)

    [Header("Line Animation Settings")]
    [SerializeField] private float lineDrawSpeed = 2f; // Speed of line drawing animation
    [SerializeField] private bool animateLines = true; // Toggle animation on/off
    [SerializeField] private AnimationCurve drawEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Easing for animation

    // Mapping dictionary for Ability -> Modifier conversions
    private static readonly Dictionary<EffectType, EffectType> abilityToModifierMap = new Dictionary<EffectType, EffectType>
    {
        { EffectType.PiercingShot, EffectType.Ricochet },
        { EffectType.BurstShot, EffectType.AttackSpeed },
        { EffectType.ExplosiveShot, EffectType.MultiShot },
        { EffectType.MeleeSwipe, EffectType.Size },
        { EffectType.AuraBurst, EffectType.DamageOverTime },
        { EffectType.Thorns, EffectType.Siphon }
    };

    // Dictionary for display names (formatted nicely)
    private static readonly Dictionary<EffectType, string> effectDisplayNames = new Dictionary<EffectType, string>
    {
        // Abilities
        { EffectType.PiercingShot, "Piercing Shot" },
        { EffectType.BurstShot, "Burst Shot" },
        { EffectType.ExplosiveShot, "Explosive Shot" },
        { EffectType.MeleeSwipe, "Melee Swipe" },
        { EffectType.AuraBurst, "Aura Burst" },
        { EffectType.Thorns, "Thorns" },
        
        // Modifiers
        { EffectType.Ricochet, "Ricochet" },
        { EffectType.AttackSpeed, "Attack Speed" },
        { EffectType.MultiShot, "Multi-Shot" },
        { EffectType.Size, "Size" },
        { EffectType.DamageOverTime, "Damage Over Time" },
        { EffectType.Siphon, "Siphon" }
    };

    // Dictionary for icons
    private Dictionary<EffectType, Sprite> effectIcons;

    private Vector3 targetPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 snapTarget;

    private NodeSlot currentSlot;
    private NodeSlot previewSlot; // The slot this node would snap to
    private NodeSlot fallbackSlot; // Fallback slot if conversion fails
    private NodeType pendingType; // Type after conversion (if any)
    private EffectType pendingEffect; // Effect after conversion (if any)
    private bool isShowingConversionPreview = false;

    // Connection visualization
    private List<LineRenderer> connectionLines = new List<LineRenderer>();
    private Dictionary<NodeSlot, LineRenderer> activeConnections = new Dictionary<NodeSlot, LineRenderer>();

    // Preview lines
    private List<LineRenderer> previewLines = new List<LineRenderer>();
    private Dictionary<NodeSlot, LineRenderer> activePreviews = new Dictionary<NodeSlot, LineRenderer>();

    // Preview node feedback
    private GameObject previewNodeInstance;
    private Image previewNodeImage; // Changed from SpriteRenderer to Image
    private Vector3 previewNodeOriginalScale;
    private float previewNodeAnimationTime = 0f;

    // Track which node owns each connection (to prevent duplicates)
    private static Dictionary<(Node, Node), LineRenderer> globalConnections = new Dictionary<(Node, Node), LineRenderer>();

    // Line animation tracking
    private Dictionary<LineRenderer, float> lineDrawStartTime = new Dictionary<LineRenderer, float>();
    private Dictionary<LineRenderer, bool> animationCompleted = new Dictionary<LineRenderer, bool>();
    private float lastSnapTime;

    // Camera tracking
    private float cameraCheckTimer = 0f;
    private float currentOrthoSize;

    // Public properties
    public NodeType Type => nodeType;
    public NodeState State => nodeState;
    public NodeSlot CurrentSlot => currentSlot;
    public EffectType Effect => effectType;

    void Awake()
    {
        InitializeIconDictionary();
        CreateLineRenderers();
        CreatePreviewLines();
        CreatePreviewNode();

        // Initialize UI on awake
        UpdateNodeUI();
    }

    void InitializeIconDictionary()
    {
        effectIcons = new Dictionary<EffectType, Sprite>
        {
            // Abilities
            { EffectType.PiercingShot, piercingShotIcon },
            { EffectType.BurstShot, burstShotIcon },
            { EffectType.ExplosiveShot, explosiveShotIcon },
            { EffectType.MeleeSwipe, meleeSwipeIcon },
            { EffectType.AuraBurst, auraBurstIcon },
            { EffectType.Thorns, thornsIcon },
            
            // Modifiers
            { EffectType.Ricochet, ricochetIcon },
            { EffectType.AttackSpeed, attackSpeedIcon },
            { EffectType.MultiShot, multiShotIcon },
            { EffectType.Size, sizeIcon },
            { EffectType.DamageOverTime, damageOverTimeIcon },
            { EffectType.Siphon, siphonIcon }
        };
    }

    void CreateLineRenderers()
    {
        // Create line renderer pool
        for (int i = 0; i < 6; i++) // Max 6 connections
        {
            GameObject lineObj = new GameObject($"Connection_{i}");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            // Set default material if none provided
            if (lineMaterial == null)
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
            else
            {
                lr.material = lineMaterial;
            }

            // Configure line renderer
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.positionCount = linePoints;
            lr.widthCurve = lineWidthCurve;
            lr.numCornerVertices = 5; // Smoother corners
            lr.numCapVertices = 5;     // Smoother ends
            lr.enabled = false;

            connectionLines.Add(lr);

            // Initialize tracking
            animationCompleted[lr] = false;
        }
    }

    void CreatePreviewLines()
    {
        // Create preview line pool
        for (int i = 0; i < maxPreviewLines; i++)
        {
            GameObject previewObj = new GameObject($"PreviewLine_{i}");
            previewObj.transform.SetParent(transform);
            previewObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = previewObj.AddComponent<LineRenderer>();

            // Set preview material
            if (previewMaterial != null)
            {
                lr.material = previewMaterial;
            }
            else if (lineMaterial != null)
            {
                lr.material = lineMaterial;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }

            // Configure preview line
            lr.startColor = previewLineColor;
            lr.endColor = previewLineColor;
            lr.startWidth = previewLineWidth;
            lr.endWidth = previewLineWidth;
            lr.positionCount = 2; // Simple straight line for preview
            lr.numCornerVertices = 5;
            lr.numCapVertices = 5;
            lr.enabled = false;

            previewLines.Add(lr);
        }
    }

    void CreatePreviewNode()
    {
        if (previewNodePrefab == null) return;

        previewNodeInstance = Instantiate(previewNodePrefab, Vector3.zero, Quaternion.identity);
        previewNodeInstance.transform.SetParent(null); // Keep in world space
        previewNodeInstance.SetActive(false);

        // Get Image component instead of SpriteRenderer
        previewNodeImage = previewNodeInstance.GetComponent<Image>();
        if (previewNodeImage == null)
        {
            Debug.LogWarning("Preview node prefab does not have an Image component!");
        }

        previewNodeOriginalScale = previewNodeInstance.transform.localScale;
    }

    void Start()
    {
        // If we already have a current slot but no name set, set it now
        if (currentSlot != null && string.IsNullOrEmpty(currentSlotName))
        {
            currentSlotName = currentSlot.SlotName;
            Debug.Log($"Set missing slot name on start: {currentSlotName}");
        }

        // CRITICAL: Skip snapping if this node was loaded from save
        // or if it's already in a slot
        if (nodeState != NodeState.InInventory && !isLoadedFromSave && currentSlot == null)
        {
            SnapToClosestAvailableSlot();
        }
    }

    public void MarkAsLoadedFromSave()
    {
        isLoadedFromSave = true;
    }

    void UpdateNodeUI()
    {
        if (!updateUIOnChange) return;

        UpdateEffectText();
        UpdateEffectIcon();
    }

    void UpdateEffectText()
    {
        if (effectText == null) return;

        // Get the display name for the current effect
        if (effectDisplayNames.TryGetValue(effectType, out string displayName))
        {
            effectText.text = displayName;
        }
        else
        {
            // Fallback to enum name if not in dictionary
            effectText.text = effectType.ToString();
        }
    }

    void UpdateEffectIcon()
    {
        if (iconImage == null) return;

        // Get the icon for the current effect
        if (effectIcons.TryGetValue(effectType, out Sprite icon))
        {
            iconImage.sprite = icon;
        }
        else
        {
            Debug.LogWarning($"No icon found for effect type: {effectType}");
        }
    }

    void UpdateUIBasedOnCamera()
    {
        if (effectText == null || iconImage == null || CamController.Instance == null) return;

        // Get current orthographic size
        currentOrthoSize = CamController.Instance.GetCurrentOrthoSize();

        // Determine which UI element to show
        bool showIcon = showIconAboveThreshold ? currentOrthoSize > cameraThreshold : currentOrthoSize < cameraThreshold;

        // Enable/disable based on threshold
        iconImage.gameObject.SetActive(showIcon);
        effectText.gameObject.SetActive(!showIcon);
    }

    // HOVER PREVIEW METHODS - Add these after UpdateUIBasedOnCamera
    public void OnHoverEnter(NodeSlot slot)
    {
        if (nodeState != NodeState.Draggable || isDragging) return;
        if (slot.state != NodeSlotState.Empty) return;

        Debug.Log($"Hover enter: {effectType} over {slot.SlotName}");

        // Store original values - make sure we capture current state
        originalEffectType = effectType;
        originalNodeType = nodeType;

        if (iconImage != null)
        {
            originalIcon = iconImage.sprite;
            originalIconColor = iconImage.color;
        }
        if (effectText != null)
        {
            originalText = effectText.text;
            originalTextColor = effectText.color;
        }

        // Check if this is a conversion case (Ability over Modifier slot)
        if (nodeType == NodeType.Ability && slot.type == SlotType.Modifier)
        {
            if (HasValidNeighborForConversion(slot) && abilityToModifierMap.TryGetValue(effectType, out EffectType modifierEffect))
            {
                // Valid conversion - show modifier preview
                Debug.Log($"Showing modifier preview: {modifierEffect}");

                if (effectDisplayNames.TryGetValue(modifierEffect, out string displayName))
                {
                    effectText.text = displayName;
                }

                if (effectIcons.TryGetValue(modifierEffect, out Sprite icon))
                {
                    iconImage.sprite = icon;
                }

                iconImage.color = conversionPreviewColor;
                effectText.color = conversionPreviewColor;
            }
            else
            {
                // Invalid conversion - show grayed out
                Debug.Log("Invalid conversion preview");
                iconImage.color = Color.gray;
                effectText.color = Color.gray;
            }
        }
        else if (nodeType == NodeType.Modifier && slot.type == SlotType.Ability)
        {
            // Modifier over ability slot - show unavailable
            iconImage.color = Color.gray;
            effectText.color = Color.gray;
        }
        else
        {
            // Valid placement - highlight
            iconImage.color = previewLineColor;
            effectText.color = previewLineColor;
        }

        isHoveringOverSlot = true;
        hoveredSlot = slot;
    }

    public void OnHoverExit()
    {
        if (!isHoveringOverSlot) return;

        Debug.Log($"Hover exit: restoring original visuals");

        // Restore original visuals
        if (iconImage != null)
        {
            iconImage.sprite = originalIcon;
            iconImage.color = originalIconColor;
        }
        if (effectText != null)
        {
            effectText.text = originalText;
            effectText.color = originalTextColor;
        }

        isHoveringOverSlot = false;
        hoveredSlot = null;
    }

    void SnapToClosestAvailableSlot()
    {
        if (NodeManager.Instance == null)
        {
            Debug.LogError("NodeManager not found in scene!");
            return;
        }

        // Find the closest available node slot that matches this node's type OR allows conversion
        NodeSlot closestSlot = FindBestAvailableSlot(transform.position);

        if (closestSlot != null)
        {
            // Handle type conversion if needed
            if (closestSlot.type == SlotType.Modifier && nodeType == NodeType.Ability)
            {
                // Ability placed in Modifier slot - will convert to Modifier
                pendingType = NodeType.Modifier;

                // Also convert the effect to its modifier counterpart
                if (abilityToModifierMap.TryGetValue(effectType, out EffectType modifierEffect))
                {
                    pendingEffect = modifierEffect;
                    Debug.Log($"Ability {effectType} will convert to Modifier {modifierEffect}");
                }
                else
                {
                    Debug.LogWarning($"No modifier counterpart found for ability {effectType}");
                    pendingEffect = effectType; // Fallback to same effect
                }
            }

            // Snap to slot position
            transform.position = closestSlot.transform.position;
            SetCurrentSlot(closestSlot);
            closestSlot.state = NodeSlotState.Occupied;
            closestSlot.OccupyingNode = this;
        }
        else
        {
            Debug.LogWarning($"No available slots found for node of type {nodeType}!");
        }
    }

    NodeSlot FindBestAvailableSlot(Vector3 position)
    {
        NodeSlot bestSlot = null;
        float bestDistance = float.MaxValue;

        foreach (NodeSlot slot in NodeManager.Instance.allNodes)
        {
            if (slot.state == NodeSlotState.Occupied) continue;

            bool slotValid = false;

            // Check if this slot type is compatible with current node
            switch (nodeType)
            {
                case NodeType.Center:
                    // Center can only go in Center slots
                    slotValid = (slot.type == SlotType.Center);
                    break;

                case NodeType.Ability:
                    // Ability can go in Ability slots OR Modifier slots (with conversion)
                    slotValid = (slot.type == SlotType.Ability || slot.type == SlotType.Modifier);
                    break;

                case NodeType.Modifier:
                    // Modifier can only go in Modifier slots
                    slotValid = (slot.type == SlotType.Modifier);
                    break;
            }

            if (!slotValid) continue;

            float distance = Vector3.Distance(position, slot.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSlot = slot;
            }
        }

        return bestSlot;
    }

    NodeSlot FindClosestAbilitySlot(Vector3 position)
    {
        NodeSlot bestSlot = null;
        float bestDistance = float.MaxValue;

        foreach (NodeSlot slot in NodeManager.Instance.allNodes)
        {
            if (slot.state == NodeSlotState.Occupied) continue;
            if (slot.type != SlotType.Ability) continue;

            float distance = Vector3.Distance(position, slot.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSlot = slot;
            }
        }

        return bestSlot;
    }

    bool CanConnectTo(Node otherNode)
    {
        if (otherNode == null) return false;

        // Get effective types (considering pending conversions)
        NodeType thisType = pendingType != NodeType.Ability ? pendingType : nodeType;
        NodeType otherType = otherNode.nodeType;

        // Connection rules based on node types
        switch (thisType)
        {
            case NodeType.Center:
                // Center can only connect to Ability
                return otherType == NodeType.Ability;

            case NodeType.Ability:
                // Ability can connect to Center and Modifier ONLY
                // NOT to other Ability nodes
                if (otherType == NodeType.Ability) return false;
                return otherType == NodeType.Center || otherType == NodeType.Modifier;

            case NodeType.Modifier:
                // Modifier can connect to Modifier and Ability ONLY
                if (otherType == NodeType.Center) return false;
                return otherType == NodeType.Modifier || otherType == NodeType.Ability;

            default:
                return false;
        }
    }

    bool HasValidNeighborForConversion(NodeSlot targetSlot)
    {
        if (targetSlot == null) return false;

        // Check all neighbors of the potential slot
        foreach (var neighborSlot in targetSlot.nearbyNodes)
        {
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                // For an Ability trying to convert to Modifier, need a nearby Ability or Modifier
                if (neighborNode.nodeType == NodeType.Ability || neighborNode.nodeType == NodeType.Modifier)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void UpdatePreviewNode()
    {
        if (previewNodeInstance == null || previewSlot == null)
        {
            if (previewNodeInstance != null)
                previewNodeInstance.SetActive(false);
            return;
        }

        // Position preview node at the target slot
        previewNodeInstance.transform.position = previewSlot.transform.position;
        previewNodeInstance.SetActive(true);

        // Update animation time
        previewNodeAnimationTime += Time.deltaTime;

        // Combined animation: pulse + zoom
        float pulse = 1f + Mathf.Sin(previewNodeAnimationTime * previewNodePulseSpeed) * previewNodePulseAmount;
        float zoom = 1f + Mathf.Sin(previewNodeAnimationTime * previewNodeZoomSpeed) * previewNodeZoomAmount;

        // Apply scale with both effects
        Vector3 newScale = previewNodeOriginalScale * previewNodeScale * pulse * zoom;
        previewNodeInstance.transform.localScale = newScale;

        // Update color based on preview type (using Image instead of SpriteRenderer)
        if (previewNodeImage != null)
        {
            Color targetColor;
            if (isShowingConversionPreview)
            {
                targetColor = previewLineColor; // Normal for fallback
            }
            else if (nodeType == NodeType.Ability && previewSlot.type == SlotType.Modifier &&
                     HasValidNeighborForConversion(previewSlot))
            {
                targetColor = conversionPreviewColor; // Magenta for conversion
            }
            else
            {
                targetColor = previewLineColor; // Normal yellow
            }

            // Pulse alpha along with the lines
            float alphaPulse = 0.8f + Mathf.Sin(previewNodeAnimationTime * pulseSpeed) * 0.2f;
            targetColor.a = pulseBaseAlpha * alphaPulse;
            previewNodeImage.color = targetColor;
        }
    }

    void UpdatePreviewLines()
    {
        if (!showPreviewWhileDragging || !isDragging)
        {
            HideAllPreviews();

            // Also clear hover preview when not dragging
            if (isHoveringOverSlot)
            {
                OnHoverExit();
            }
            return;
        }

        // Find the best available slot (where this node would snap)
        if (NodeManager.Instance != null)
        {
            NodeSlot newPreviewSlot = FindBestAvailableSlot(transform.position);
            fallbackSlot = FindClosestAbilitySlot(transform.position);

            // Handle hover preview when preview slot changes
            if (newPreviewSlot != previewSlot)
            {
                // Exit previous hover
                if (isHoveringOverSlot)
                {
                    OnHoverExit();
                }

                // Update preview slot
                previewSlot = newPreviewSlot;

                // Enter new hover if we have a valid slot
                if (previewSlot != null)
                {
                    OnHoverEnter(previewSlot);
                }
            }
            // If we have a preview slot but somehow not hovering, enter hover
            else if (previewSlot != null && !isHoveringOverSlot)
            {
                OnHoverEnter(previewSlot);
            }
            // If no preview slot but we're hovering, exit hover
            else if (previewSlot == null && isHoveringOverSlot)
            {
                OnHoverExit();
            }
        }

        if (previewSlot == null)
        {
            HideAllPreviews();
            return;
        }

        // Rest of your existing UpdatePreviewLines code...
        int previewIndex = 0;
        isShowingConversionPreview = false;

        // For Ability nodes trying to go into Modifier slots, check if conversion is valid
        bool conversionValid = true;
        if (nodeType == NodeType.Ability && previewSlot.type == SlotType.Modifier)
        {
            conversionValid = HasValidNeighborForConversion(previewSlot);

            // If conversion not valid, use fallback slot for previews
            if (!conversionValid && fallbackSlot != null)
            {
                isShowingConversionPreview = true;
                previewSlot = fallbackSlot;
            }
        }

        // Update the preview node
        UpdatePreviewNode();

        // Determine effective type for connection rules (consider conversion)
        NodeType effectiveType;
        Color previewColorToUse = previewLineColor;

        if (isShowingConversionPreview)
        {
            // When showing fallback, we're still an Ability
            effectiveType = NodeType.Ability;
            previewColorToUse = previewLineColor; // Normal color for ability slot preview
        }
        else if (nodeType == NodeType.Ability && previewSlot.type == SlotType.Modifier && conversionValid)
        {
            // Valid conversion preview
            effectiveType = NodeType.Modifier;
            previewColorToUse = conversionPreviewColor;
        }
        else
        {
            // Normal preview
            effectiveType = nodeType;
            previewColorToUse = previewLineColor;
        }

        // Store original type and temporarily set effective type for connection checks
        NodeType originalType = nodeType;
        pendingType = (effectiveType != originalType) ? effectiveType : NodeType.Ability;

        // Check all neighbors of that potential slot
        foreach (var neighborSlot in previewSlot.nearbyNodes)
        {
            if (previewIndex >= maxPreviewLines) break;

            // If neighbor exists and has an occupying node
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                // Check if this connection is allowed by type rules (using effective type)
                if (!CanConnectTo(neighborNode))
                {
                    continue; // Skip - connection not allowed by type rules
                }

                // Check if this connection already exists in global connections
                var connectionKey = (this, neighborNode);
                var reverseKey = (neighborNode, this);

                // Only show preview for connections that don't already exist
                if (!globalConnections.ContainsKey(connectionKey) && !globalConnections.ContainsKey(reverseKey))
                {
                    LineRenderer previewLine = previewLines[previewIndex];
                    previewLine.enabled = true;

                    // Draw from the slot position (not the node position)
                    previewLine.SetPosition(0, previewSlot.transform.position);
                    previewLine.SetPosition(1, neighborNode.transform.position);

                    // Pulse effect while dragging - with separate speed and intensity controls
                    float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed + previewIndex) * pulseIntensity;
                    Color pulsedColor = previewColorToUse;
                    pulsedColor.a = pulseBaseAlpha * pulse;
                    previewLine.startColor = pulsedColor;
                    previewLine.endColor = pulsedColor;

                    // Store in active previews
                    activePreviews[neighborSlot] = previewLine;

                    previewIndex++;
                }
            }
        }

        // Reset pending type
        pendingType = NodeType.Ability;

        // Hide any unused preview lines
        for (int i = previewIndex; i < previewLines.Count; i++)
        {
            previewLines[i].enabled = false;
        }
    }

    void HideAllPreviews()
    {
        foreach (var preview in previewLines)
        {
            preview.enabled = false;
        }

        if (previewNodeInstance != null)
        {
            previewNodeInstance.SetActive(false);
        }

        activePreviews.Clear();
        previewSlot = null;
        fallbackSlot = null;
        isShowingConversionPreview = false;
    }

    void OnMouseDown()
    {
        // Check if node is in a state that allows dragging
        // Only Draggable and InInventory nodes can be dragged
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        offset = transform.position - GetMouseWorldPos();
        targetPosition = transform.position;
        isDragging = true;
        isSnapping = false;

        // Hide connections while dragging
        HideAllConnections();
        HideAllPreviews();

        // Remove this node's connections from global dictionary
        RemoveGlobalConnections();

        // If we're in a slot (not inventory), notify neighbors and free the slot
        if (currentSlot != null)
        {
            // Notify neighbors to update their connections (since this node is leaving)
            NotifyNeighborsToUpdateConnections();

            // Free up current slot
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;
            currentSlot = null;
        }
        // If we're in inventory, we don't need to do anything special - just start dragging
    }

    void OnMouseDrag()
    {
        // Check if node is in a state that allows dragging
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        targetPosition = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        // Check if node is in a state that allows dragging
        if (nodeState != NodeState.Draggable && nodeState != NodeState.InInventory) return;

        isDragging = false;

        // Hide preview lines and node
        HideAllPreviews();

        // Handle different states on release
        switch (nodeState)
        {
            case NodeState.InInventory:
                // In inventory mode - when released, we should snap like a draggable node
                // So we fall through to the Draggable case
                nodeState = NodeState.Draggable; // Set to draggable for future reference
                goto case NodeState.Draggable; // Fall through to handle snapping

            case NodeState.Draggable:
                // Find best available slot for this node
                if (NodeManager.Instance != null)
                {
                    NodeSlot bestSlot = FindBestAvailableSlot(transform.position);
                    NodeSlot fallbackAbilitySlot = FindClosestAbilitySlot(transform.position);

                    if (bestSlot != null)
                    {
                        // For Ability trying to go into Modifier slot
                        if (nodeType == NodeType.Ability && bestSlot.type == SlotType.Modifier)
                        {
                            // Check if conversion is valid
                            if (HasValidNeighborForConversion(bestSlot))
                            {
                                // Valid conversion - use the Modifier slot
                                snapTarget = bestSlot.transform.position;
                                SetCurrentSlot(bestSlot);
                                pendingType = NodeType.Modifier;

                                // Set pending effect to modifier counterpart
                                if (abilityToModifierMap.TryGetValue(effectType, out EffectType modifierEffect))
                                {
                                    pendingEffect = modifierEffect;
                                }

                                Debug.Log($"Ability {effectType} converting to Modifier {pendingEffect}!");
                            }
                            else
                            {
                                // Invalid conversion - use fallback Ability slot if available
                                if (fallbackAbilitySlot != null)
                                {
                                    snapTarget = fallbackAbilitySlot.transform.position;
                                    SetCurrentSlot(fallbackAbilitySlot);
                                    pendingType = NodeType.Ability; // No conversion
                                    pendingEffect = effectType; // Keep same effect
                                    Debug.Log("Cannot convert - snapping to nearest Ability slot instead");
                                }
                                else
                                {
                                    // No fallback available, use original slot
                                    snapTarget = bestSlot.transform.position;
                                    SetCurrentSlot(bestSlot);
                                    pendingType = NodeType.Ability;
                                    pendingEffect = effectType;
                                    Debug.Log("No Ability slots available - using Modifier slot without conversion (will this work?)");
                                }
                            }
                        }
                        else
                        {
                            // Normal snap - no conversion
                            snapTarget = bestSlot.transform.position;
                            SetCurrentSlot(bestSlot);
                            pendingType = NodeType.Ability;
                            pendingEffect = effectType;
                        }

                        isSnapping = true;
                    }
                    else
                    {
                        Debug.LogWarning($"No available slots found for node of type {nodeType}!");
                    }
                }
                break;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                dragSpeed * Time.deltaTime
            );

            // Update preview lines while dragging
            UpdatePreviewLines();
        }
        else if (isSnapping)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                snapTarget,
                snapSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                transform.position = snapTarget;
                isSnapping = false;
                lastSnapTime = Time.time;

                // Apply type conversion if pending
                if (pendingType != NodeType.Ability)
                {
                    nodeType = pendingType;

                    // Apply effect conversion if pending
                    if (pendingEffect != effectType)
                    {
                        effectType = pendingEffect;

                        // UPDATE THE UI HERE!
                        UpdateNodeUI();

                        Debug.Log($"Effect converted to {effectType}");
                    }

                    Debug.Log($"Node converted to {nodeType}");
                }

                // Reset pending values
                pendingType = NodeType.Ability;
                pendingEffect = effectType;

                // Reset animation completion flags for new connections
                foreach (var lr in connectionLines)
                {
                    animationCompleted[lr] = false;
                }

                if (currentSlot != null)
                {
                    currentSlot.state = NodeSlotState.Occupied;
                    currentSlot.OccupyingNode = this;

                    // Update connections after snapping - pass true for new connection
                    UpdateConnections(true);
                    NotifyNeighborsToUpdateConnections();

                    // Notify NodeManager that state changed
                    if (NodeManager.Instance != null)
                    {
                        NodeManager.Instance.OnNodeStateChanged();
                    }

                    // LOCK THE NODE AFTER SNAPPING IS COMPLETE
                    nodeState = NodeState.Locked;

                    if (BuildManager.Instance != null)
                    {
                        BuildManager.Instance.OnNodeStateChanged();
                    }

                    Debug.Log($"Node locked in place at {currentSlot.transform.position} with effect {effectType}");
                }
            }
        }

        // Update line animations
        if (animateLines)
        {
            UpdateLineAnimations();
        }

        // Update camera-based UI with throttling
        cameraCheckTimer += Time.deltaTime;
        if (cameraCheckTimer >= checkInterval)
        {
            cameraCheckTimer = 0f;
            UpdateUIBasedOnCamera();
        }
    }

    void UpdateLinePoints(LineRenderer lr, Vector3 start, Vector3 end, float progress = 1f)
    {
        if (lr.positionCount < 2) return;

        Vector3[] points = new Vector3[lr.positionCount];

        // Apply easing to progress
        float easedProgress = drawEasingCurve.Evaluate(Mathf.Clamp01(progress));
        float totalDistance = Vector3.Distance(start, end);
        float drawnDistance = totalDistance * easedProgress;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)(lr.positionCount - 1);

            if (animateLines && progress < 1f)
            {
                // For animation, only draw up to the progress point
                float pointDistance = t * totalDistance;
                if (pointDistance <= drawnDistance)
                {
                    // Point is within drawn portion
                    points[i] = Vector3.Lerp(start, end, t);
                }
                else
                {
                    // Point is beyond drawn portion - clamp to end of drawn portion
                    float clampedT = drawnDistance / totalDistance;
                    points[i] = Vector3.Lerp(start, end, clampedT);
                }
            }
            else
            {
                // Full line
                points[i] = Vector3.Lerp(start, end, t);
            }
        }

        lr.SetPositions(points);
    }

    void UpdateLineAnimations()
    {
        List<(Node, Node)> toRemove = new List<(Node, Node)>();
        bool anyAnimationJustCompleted = false;

        foreach (var kvp in globalConnections)
        {
            var connection = kvp.Key;
            LineRenderer lr = kvp.Value;

            Node nodeA = connection.Item1;
            Node nodeB = connection.Item2;

            // Check if connection is still valid
            if (nodeA == null || nodeB == null || nodeA.gameObject == null || nodeB.gameObject == null)
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Check if nodes are still in adjacent slots
            if (nodeA.CurrentSlot == null || nodeB.CurrentSlot == null)
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Check if slots are still neighbors
            if (!AreSlotsNeighbors(nodeA.CurrentSlot, nodeB.CurrentSlot))
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Check if connection is still allowed by type rules
            if (!nodeA.CanConnectTo(nodeB))
            {
                if (lr != null) lr.enabled = false;
                toRemove.Add(connection);
                continue;
            }

            // Update line if renderer exists
            if (lr != null && lr.enabled)
            {
                // Determine which node is newer to animate from it
                float timeSinceSnapA = Time.time - nodeA.lastSnapTime;
                float timeSinceSnapB = Time.time - nodeB.lastSnapTime;

                if (timeSinceSnapA < timeSinceSnapB && timeSinceSnapA < 1f)
                {
                    // Animate from node A to node B
                    float progress = Mathf.Clamp01(timeSinceSnapA * lineDrawSpeed);
                    UpdateLinePoints(lr, nodeA.transform.position, nodeB.transform.position, progress);

                    // Check if animation just completed
                    if (progress >= 0.99f && !nodeA.animationCompleted[lr])
                    {
                        nodeA.animationCompleted[lr] = true;
                        anyAnimationJustCompleted = true;
                    }
                }
                else if (timeSinceSnapB < 1f)
                {
                    // Animate from node B to node A
                    float progress = Mathf.Clamp01(timeSinceSnapB * lineDrawSpeed);
                    UpdateLinePoints(lr, nodeB.transform.position, nodeA.transform.position, progress);

                    // Check if animation just completed
                    if (progress >= 0.99f && !nodeB.animationCompleted[lr])
                    {
                        nodeB.animationCompleted[lr] = true;
                        anyAnimationJustCompleted = true;
                    }
                }
                else
                {
                    // No animation, just draw full line
                    UpdateLinePoints(lr, nodeA.transform.position, nodeB.transform.position, 1f);
                }
            }
        }

        // Clean up invalid connections
        foreach (var connection in toRemove)
        {
            globalConnections.Remove(connection);
        }

        // --- ANIMATION COMPLETION HOOK ---
        // Add your logic here when any animation completes
        if (anyAnimationJustCompleted && NodeManager.Instance != null)
        {
            ShakeManager.Instance.shakeCam(2.5f, 1f, 0.5f);
        }
    }

    bool AreSlotsNeighbors(NodeSlot slotA, NodeSlot slotB)
    {
        if (slotA == null || slotB == null) return false;

        // Check if slotB is in slotA's nearbyNodes list
        return slotA.nearbyNodes.Contains(slotB);
    }

    void RemoveGlobalConnections()
    {
        List<(Node, Node)> toRemove = new List<(Node, Node)>();

        foreach (var kvp in globalConnections)
        {
            if (kvp.Key.Item1 == this || kvp.Key.Item2 == this)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.enabled = false;
                }
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var connection in toRemove)
        {
            globalConnections.Remove(connection);
        }
    }

    public void UpdateConnections(bool isNewConnection = false)
    {
        if (currentSlot == null)
        {
            HideAllConnections();
            return;
        }

        // Check each neighbor slot
        for (int i = 0; i < currentSlot.nearbyNodes.Count; i++)
        {
            NodeSlot neighborSlot = currentSlot.nearbyNodes[i];

            // If neighbor exists and has an occupying node
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                Node neighborNode = neighborSlot.OccupyingNode;

                // Check if this connection is allowed by type rules
                if (!CanConnectTo(neighborNode))
                {
                    continue; // Skip - connection not allowed by type rules
                }

                // Create a unique key for this connection (always use consistent ordering)
                var connectionKey = (this, neighborNode);
                var reverseKey = (neighborNode, this);

                // Check if this connection already exists (either direction)
                if (globalConnections.ContainsKey(connectionKey) || globalConnections.ContainsKey(reverseKey))
                {
                    continue; // Skip - connection already handled by other node
                }

                // Find an available line renderer
                LineRenderer lr = GetAvailableLineRenderer();
                if (lr != null)
                {
                    lr.enabled = true;

                    // Update line renderer settings
                    lr.widthCurve = lineWidthCurve;
                    lr.startColor = lineColor;
                    lr.endColor = lineColor;

                    if (isNewConnection && animateLines)
                    {
                        // Record start time for animation
                        lineDrawStartTime[lr] = Time.time;
                        animationCompleted[lr] = false;

                        // Start with just the start point
                        Vector3[] startPoints = new Vector3[linePoints];
                        for (int j = 0; j < linePoints; j++)
                        {
                            startPoints[j] = transform.position;
                        }
                        lr.SetPositions(startPoints);
                    }
                    else
                    {
                        // Set full line immediately
                        UpdateLinePoints(lr, transform.position, neighborNode.transform.position, 1f);
                        animationCompleted[lr] = true;
                    }

                    // Store in global connections (use consistent ordering)
                    globalConnections[connectionKey] = lr;
                }
            }
        }
    }

    LineRenderer GetAvailableLineRenderer()
    {
        foreach (var lr in connectionLines)
        {
            if (!lr.enabled)
            {
                return lr;
            }
        }
        return null;
    }

    void NotifyNeighborsToUpdateConnections()
    {
        if (currentSlot == null) return;

        foreach (var neighborSlot in currentSlot.nearbyNodes)
        {
            if (neighborSlot != null && neighborSlot.OccupyingNode != null)
            {
                neighborSlot.OccupyingNode.UpdateConnections();
            }
        }
    }

    void HideAllConnections()
    {
        foreach (var lr in connectionLines)
        {
            lr.enabled = false;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnDestroy()
    {
        // Remove this node's connections from global dictionary
        RemoveGlobalConnections();

        // Notify neighbors before destroying
        if (currentSlot != null)
        {
            // Store neighbors before clearing
            List<NodeSlot> neighbors = new List<NodeSlot>(currentSlot.GetValidNeighbors());

            // Free up slot
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;

            // Notify neighbors to update
            foreach (var neighborSlot in neighbors)
            {
                if (neighborSlot != null && neighborSlot.OccupyingNode != null)
                {
                    neighborSlot.OccupyingNode.UpdateConnections();
                }
            }
        }

        // Clean up connection lines
        foreach (var lr in connectionLines)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }

        // Clean up preview lines
        foreach (var preview in previewLines)
        {
            if (preview != null)
                Destroy(preview.gameObject);
        }

        // Clean up preview node
        if (previewNodeInstance != null)
            Destroy(previewNodeInstance);

        // Notify NodeManager
        if (NodeManager.Instance != null)
        {
            NodeManager.Instance.OnNodeStateChanged();
        }

        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.OnNodeStateChanged();
        }
    }

    // Public methods to change node state
    public void SetNodeState(NodeState newState)
    {
        nodeState = newState;
    }

    public void SetNodeType(NodeType newType)
    {
        nodeType = newType;
    }

    public void SetEffectType(EffectType newEffect)
    {
        effectType = newEffect;
        UpdateNodeUI(); // Update UI when manually changing effect
    }

    // Method to unlock a node (make it draggable again)
    public void UnlockNode()
    {
        if (nodeState == NodeState.Locked)
        {
            nodeState = NodeState.Draggable;
            Debug.Log("Node unlocked");
        }
    }

    // Method to place node in inventory (sets state and optionally moves it)
    public void MoveToInventory(Vector3 inventoryPosition)
    {
        // Remove connections from global dictionary
        RemoveGlobalConnections();

        // Free up current slot if occupied
        if (currentSlot != null)
        {
            currentSlot.state = NodeSlotState.Empty;
            currentSlot.OccupyingNode = null;

            // Notify neighbors to update
            NotifyNeighborsToUpdateConnections();

            currentSlot = null;
        }

        // Hide connections
        HideAllConnections();

        // Hide preview lines
        HideAllPreviews();

        // Move to inventory position
        transform.position = inventoryPosition;

        // Set state to InInventory
        nodeState = NodeState.InInventory;
    }

    // Method to refresh line renderer settings (call after changing inspector values at runtime)
    public void RefreshLineSettings()
    {
        foreach (var lr in connectionLines)
        {
            lr.widthCurve = lineWidthCurve;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.positionCount = linePoints;
        }

        // Redraw connections
        UpdateConnections();
    }

    // Helper method to check if this node can convert to a modifier
    public bool CanConvertToModifier()
    {
        return nodeType == NodeType.Ability && abilityToModifierMap.ContainsKey(effectType);
    }

    // Helper method to get the modifier counterpart for this ability
    public EffectType? GetModifierCounterpart()
    {
        if (abilityToModifierMap.TryGetValue(effectType, out EffectType modifierEffect))
        {
            return modifierEffect;
        }
        return null;
    }

    public void SetCurrentSlot(NodeSlot slot)
    {
        Debug.Log($"SetCurrentSlot called for {effectType} with slot: {(slot != null ? slot.SlotName : "null")}");

        // Clear previous slot if any
        if (currentSlot != null && currentSlot.OccupyingNode == this)
        {
            Debug.Log($"Clearing previous slot: {currentSlot.SlotName}");
            currentSlot.OccupyingNode = null;
            currentSlot.state = NodeSlotState.Empty;
        }

        // Set new slot
        currentSlot = slot;

        if (slot != null)
        {
            currentSlotName = slot.SlotName; // THIS IS THE KEY LINE
            slot.OccupyingNode = this;
            slot.state = NodeSlotState.Occupied;

            // Update position to slot position
            transform.position = slot.transform.position;

            Debug.Log($"Node {effectType} now in slot: {currentSlotName}");
        }
        else
        {
            currentSlotName = "";
            Debug.Log($"Node {effectType} removed from slot");
        }
    }

    public void InitializeLoadedNode(NodeSlot slot, NodeState newState)
    {
        // Set the state first
        nodeState = newState;

        // Clear any pending operations
        isDragging = false;
        isSnapping = false;

        // Set the slot
        if (slot != null)
        {
            // Directly set currentSlot without triggering any side effects
            currentSlot = slot;

            // Update slot references
            slot.OccupyingNode = this;
            slot.state = NodeSlotState.Occupied;

            // Set position to slot position
            transform.position = slot.transform.position;

            // Ensure node is locked if in slot
            if (newState == NodeState.Locked)
            {
                nodeState = NodeState.Locked;
            }
        }

        // Update UI
        UpdateNodeUI();
    }

    public void InitializeFromSave(NodeSlot slot, NodeState newState)
    {
        // CRITICAL: Set these BEFORE anything else
        nodeState = newState;

        // Clear all flags
        isDragging = false;
        isSnapping = false;
        pendingType = NodeType.Ability;
        pendingEffect = effectType;

        // Set the slot directly without triggering any callbacks
        if (slot != null)
        {
            // Direct field assignment (bypass property)
            currentSlot = slot;

            // Update slot references
            slot.OccupyingNode = this;
            slot.state = NodeSlotState.Occupied;

            // Set position to slot position
            transform.position = slot.transform.position;

            // Ensure state is Locked if in slot
            if (newState == NodeState.Locked)
            {
                nodeState = NodeState.Locked;
            }
        }

        // Update UI
        UpdateNodeUI();

        // Log for verification
        Debug.Log($"Node {effectType} initialized in slot at {slot.transform.position}");
    }
}