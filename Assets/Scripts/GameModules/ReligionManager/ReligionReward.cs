using UnityEngine;
using System;

[Serializable]
public abstract class ReligionReward
{
    public abstract void ApplyReward();
    public abstract void RemoveReward();
}

[Serializable]
public enum ReligionRewardType
{
    StartOfCombatStatBuff,
    StartOfCombatSecondaryStatBuff,
    RandomItem,
    SpecificItem,
    StatPoints,
    HealingMultiplier,
    AilmentImmunity,
    AilmentOnEnemy,
    Passive,
    FreeRepairAtBlackSmith,
    UpgradeEquipment,
    ManaRecovery,
    StatBuff,
}

[Serializable]
public struct ReligionRewardData
{
    public ReligionRewardType type;
    public float value;
}

[Serializable]
public struct PrimaryStatWithValue
{
    public Stat stat;
    public float value;
}

[Serializable]
public struct SecondaryStatWithValue
{
    public SecondaryStat stat;
    public float value;
}

[Serializable]
public struct SpecificItemRewardData
{
    public int itemID;
}

[Serializable]
public struct RandomItemRewardData
{
    public EquipmentSlot slot;
    public Rarity rarity;
}

[Serializable]
public struct AilmentOnEnemyRewardData
{
    public AilmentType ailmentType;
    public float chance;
    public int maxEnemies;
    public int encounterCooldown;
}
