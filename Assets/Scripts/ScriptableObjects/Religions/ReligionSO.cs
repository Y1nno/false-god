using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Religion", menuName = "Scriptable Objects/Religion")]
public class ReligionSO : ScriptableObject
{
    public string religionName;
    public string description;
    public Sprite icon;

    public List<ReligionLevel> levels;

}

[Serializable]
public class ReligionLevel
{
    // Requirement (Quest)
    public QuestType requirement;

    // Requirement params (fill based on requirement)
    public string requiredReligionName; // for JoinReligion (swap to ReligionSO if you have it)
    public int requiredItemID;          // for GiveItem
    public int requiredItemAmount;      // for GiveItem
    public int bossID;                  // for KillBoss

    // Reward
    public ReligionRewardType rewardType;

    // Reward params (fill based on rewardType)
    public float value;        // DodgeChance (or general numeric)
    public int specificItemID; // SpecificItem
    public EquipmentSlot randomSlot;
    public Rarity randomRarity;
    public PrimaryStatWithValue startOfCombatStatBuff;
    public SecondaryStatWithValue startOfCombatSecondaryStatBuff;
    public int StatPointsToAllocate;
    public float HealingMultiplier;
    public AilmentType ailmentImmunity;
    public AilmentOnEnemyRewardData ailmentOnEnemy;
    public Passive passive;
    public int FreeRepairsAtBlackSmith;
    public int UpgradeEquipment;
    public float manaRecoveryPercent;
    public PrimaryStatWithValue statBuff;
}