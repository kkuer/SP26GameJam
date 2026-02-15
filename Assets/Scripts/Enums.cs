using UnityEngine;

public enum NodeSlotState
{
    Occupied,
    Empty
}

public enum NodeState
{
    Draggable,
    Locked,
    InInventory
}

public enum SlotType
{
    Center,
    Ability,
    Modifier
}

public enum NodeType
{
    Center,
    Ability,
    Modifier
}

public enum EffectType
{
    //abilities
    PiercingShot,
    BurstShot,
    ExplosiveShot,
    MeleeSwipe,
    AuraBurst,
    Thorns,

    //modifiers
    Ricochet,
    AttackSpeed,
    MultiShot,
    Size,
    DamageOverTime,
    Siphon
}

public enum NodeColor
{
    Red,
    Blue,
    Green,
    Purple,
    Yellow,
    Orange
}