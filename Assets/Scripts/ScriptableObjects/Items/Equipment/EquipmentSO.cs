using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "Scriptable Objects/Items/Equipment")]
public class EquipmentSO : ItemSO
{
    [Header("Equipment Specific")]
    [Tooltip("The equipment slot this item can be equipped in")]
    public EquipmentSlot Slot;
    public Rarity Rarity;
    public BoundedInt PhysicalDefense;
    public BoundedInt SpecialDefense;
    public BoundedInt STR;
    public BoundedInt DEX;
    public BoundedInt INT;
    public BoundedInt SPD;
    public BoundedInt CritChance;
    public BoundedInt BlockChance;
    public BoundedInt DodgeChance;
    public List<TraitWithValue> Traits;
    public CombatActionSO Skill;
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

    public TraitWithValue(EquipmentTrait trait, int value)
    {
        Trait = trait;
        Value = value;
    }
}