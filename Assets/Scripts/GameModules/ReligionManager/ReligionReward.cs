using UnityEngine;
using System;

[Serializable]
public abstract class ReligionRewardSO : ScriptableObject
{
    public abstract void ApplyReward();
    public abstract void RemoveReward();
}

[CreateAssetMenu(fileName = "StartOfCombatStatBuff", menuName = "Scriptable Objects/Religion Rewards/Start Of Combat Stat Buff")]
public class StartOfCombatStatBuffReligionRewardSO : ReligionRewardSO
{
    public Stat stat;
    public float value;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "StartOfCombatSecondaryStatBuff", menuName = "Scriptable Objects/Religion Rewards/Start Of Combat Secondary Stat Buff")]
public class StartOfCombatSecondaryStatBuffReligionRewardSO : ReligionRewardSO
{
    public SecondaryStat stat;
    public float value;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "RandomItemReward", menuName = "Scriptable Objects/Religion Rewards/Random Item")]
public class RandomItemReligionRewardSO : ReligionRewardSO
{
    public EquipmentSlot slot;
    public Rarity rarity;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "SpecificItemReward", menuName = "Scriptable Objects/Religion Rewards/Specific Item")]
public class SpecificItemReligionRewardSO : ReligionRewardSO
{
    public int itemID;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "StatPointsReward", menuName = "Scriptable Objects/Religion Rewards/Stat Points")]
public class StatPointsReligionRewardSO : ReligionRewardSO
{
    public int points;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "HealingMultiplierReward", menuName = "Scriptable Objects/Religion Rewards/Healing Multiplier")]
public class HealingMultiplierReligionRewardSO : ReligionRewardSO
{
    public float multiplier;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "AilmentOnEnemyReward", menuName = "Scriptable Objects/Religion Rewards/Ailment On Enemy")]
public class AilmentOnEnemyReligionRewardSO : ReligionRewardSO
{
    public AilmentType ailmentType;
    public float chance;
    public int maxEnemies;
    public int encounterCooldown;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "PassiveReward", menuName = "Scriptable Objects/Religion Rewards/Passive")]
public class PassiveReligionRewardSO : ReligionRewardSO
{
    public Passive passive;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "FreeRepairAtBlacksmithReward", menuName = "Scriptable Objects/Religion Rewards/Free Repair At Blacksmith")]
public class FreeRepairAtBlacksmithReligionRewardSO : ReligionRewardSO
{
    public int freeRepairs;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "UpgradeEquipmentReward", menuName = "Scriptable Objects/Religion Rewards/Upgrade Equipment")]
public class UpgradeEquipmentReligionRewardSO : ReligionRewardSO
{
    public int upgradeAmount;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "ManaRecoveryReward", menuName = "Scriptable Objects/Religion Rewards/Mana Recovery")]
public class ManaRecoveryReligionRewardSO : ReligionRewardSO
{
    [Range(0f, 1f)]
    public float manaRecoveryPercent;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}

[CreateAssetMenu(fileName = "StatBuffReward", menuName = "Scriptable Objects/Religion Rewards/Stat Buff")]
public class StatBuffReligionRewardSO : ReligionRewardSO
{
    public Stat stat;
    public float value;

    public override void ApplyReward() { }
    public override void RemoveReward() { }
}
