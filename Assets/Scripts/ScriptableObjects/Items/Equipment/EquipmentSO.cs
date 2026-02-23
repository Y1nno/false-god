using System.Collections.Generic;
using UnityEngine;

public enum CooldownType { None, CombatRounds, Encounters }

[CreateAssetMenu(fileName = "Equipment", menuName = "Scriptable Objects/Items/Equipment")]
public class EquipmentSO : ItemSO
{
    [Header("Equipment Specific")]
    [Tooltip("The equipment slot this item can be equipped in")]
    public EquipmentSlot Slot;
    public Rarity Rarity;
    public BoundedInt PhysicalDefense;
    public BoundedInt SpecialDefense;
    public BoundedInt PhysicalAttack;
    public BoundedInt SpecialAttack;
    public BoundedInt STR;
    public BoundedInt DEX;
    public BoundedInt INT;
    public BoundedInt SPD;
    public BoundedInt CritChance;
    public BoundedInt BlockChance;
    public BoundedInt DodgeChance;
    public List<TraitWithValue> Traits;
    public CombatActionSO Skill;

    /// <summary>
    /// Creates a cloned instance of this EquipmentSO where all BoundedInt stats
    /// are rolled between their min and max to assign a final static 'value'.
    /// </summary>
    public EquipmentSO InstantiateAndRollStats()
    {
        EquipmentSO instance = Instantiate(this);
        instance.name = this.name; // Clean up the "(Clone)" suffix if desired

        // Action to roll a single BoundedInt and lock its value
        void RollStat(ref BoundedInt stat)
        {
            if (stat.value == 0 && (stat.min > 0 || stat.max > 0))
            {
                stat.value = stat.Roll();
                // We set min/max to the value so it acts as a static rolled number from here on
                stat.min = stat.value;
                stat.max = stat.value;
            }
        }

        RollStat(ref instance.PhysicalDefense);
        RollStat(ref instance.SpecialDefense);
        RollStat(ref instance.PhysicalAttack);
        RollStat(ref instance.SpecialAttack);
        RollStat(ref instance.STR);
        RollStat(ref instance.DEX);
        RollStat(ref instance.INT);
        RollStat(ref instance.SPD);
        RollStat(ref instance.CritChance);
        RollStat(ref instance.BlockChance);
        RollStat(ref instance.DodgeChance);

        // Deep copy the Traits list so instances don't share memory references
        if (this.Traits != null)
        {
            instance.Traits = new List<TraitWithValue>(this.Traits);
        }

        return instance;
    }
}

[System.Serializable]
public struct BoundedInt
{
    [Tooltip("The current value of the stat, invalidates min and max if set")]
    public int value;
    [Tooltip("The minimum possible value of the stat, invalidated if current value is nonzero")]
    public int min;
    [Tooltip("The maximum possible value of the stat, invalidated if current value is nonzero")]
    public int max;

    public BoundedInt(int min, int max, int value)
    {
        this.min = min;
        this.max = max;
        this.value = value;
        Validate();
    }

    public void Validate()
    {
        if (max < min) max = min;
        value = Mathf.Clamp(value, min, max);
    }

    public int Roll() => Random.Range(min, max + 1);
}

[System.Serializable]
public struct TraitWithValue
{
    public EquipmentTrait Trait;
    public int Value;
    
    // Fields specific to MoveFirst
    public CooldownType CooldownType;
    public int CooldownDuration;
    [HideInInspector]
    public int CurrentCooldown;

    public TraitWithValue(EquipmentTrait trait, int value)
    {
        Trait = trait;
        Value = value;
        CooldownType = CooldownType.None;
        CooldownDuration = 0;
        CurrentCooldown = 0;
    }
}