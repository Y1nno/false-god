using UnityEngine;
using System.Collections.Generic;

public class Equipment : Item
{
    public EquipmentSO BaseData { get; private set; }
    public string ItemName { get; private set; }
    public override string GetName() => ItemName;
    public EquipmentSlot Slot { get; private set; }
    public Rarity Rarity { get; private set; }
    public new bool IsTwoHanded { get; private set; }
    public int PhysicalDefense { get; private set; }
    public int SpecialDefense { get; private set; }
    public int SpecialAttack { get; private set; }
    public int PhysicalAttack { get; private set; }
    public int STR { get; private set; }
    public int DEX { get; private set; }
    public int INT { get; private set; }
    public int SPD { get; private set; }
    public int CritChance { get; private set; }
    public int BlockChance { get; private set; }
    public int BlockAmount { get; private set; }
    public int DodgeChance { get; private set; }
    
    // Durability System
    public int MaxDurability { get; private set; }
    public int CurrentDurability { get; private set; }

    public List<TraitWithValue> Traits { get; private set; }
    public CombatAction Skill { get; private set; }

    public Equipment(EquipmentSO baseData)
    {
        BaseData = baseData;
        ItemName = baseData.ItemName;
        Slot = baseData.Slot;
        Rarity = baseData.Rarity;
        IsTwoHanded = baseData.IsTwoHanded;

        PhysicalDefense = baseData.PhysicalDefense.value != 0 ? baseData.PhysicalDefense.value : baseData.PhysicalDefense.max;
        SpecialDefense = baseData.SpecialDefense.value != 0 ? baseData.SpecialDefense.value : baseData.SpecialDefense.max;
        PhysicalAttack = baseData.PhysicalAttack.value != 0 ? baseData.PhysicalAttack.value : baseData.PhysicalAttack.max;
        SpecialAttack = baseData.SpecialAttack.value != 0 ? baseData.SpecialAttack.value : baseData.SpecialAttack.max;
        STR = baseData.STR.value != 0 ? baseData.STR.value : baseData.STR.max;
        DEX = baseData.DEX.value != 0 ? baseData.DEX.value : baseData.DEX.max;
        INT = baseData.INT.value != 0 ? baseData.INT.value : baseData.INT.max;
        SPD = baseData.SPD.value != 0 ? baseData.SPD.value : baseData.SPD.max;
        CritChance = baseData.CritChance.value != 0 ? baseData.CritChance.value : baseData.CritChance.max;
        BlockChance = baseData.BlockChance.value != 0 ? baseData.BlockChance.value : baseData.BlockChance.max;
        BlockAmount = baseData.BlockAmount.value != 0 ? baseData.BlockAmount.value : baseData.BlockAmount.max;
        DodgeChance = baseData.DodgeChance.value != 0 ? baseData.DodgeChance.value : baseData.DodgeChance.max;

        MaxDurability = baseData.MaxDurability;
        CurrentDurability = baseData.CurrentDurability;

        Traits = baseData.Traits != null ? new List<TraitWithValue>(baseData.Traits) : new List<TraitWithValue>();
        
        // Note: We leave Skill null for runtime resolution if we aren't instantiating combat actions here directly.
    }

    /// <summary>
    /// Returns 0.5f (halving stats) if Durability is at or below 30%, otherwise returns 1.0f.
    /// </summary>
    public float GetDurabilityMultiplier()
    {
        if (MaxDurability <= 0) return 1.0f; // Safety against uninitiated items
        return ((float)CurrentDurability / MaxDurability <= 0.3f) ? 0.5f : 1.0f;
    }

    /// <summary>
    /// Reduces current durability by the given amount.
    /// Returns TRUE if durability hit 0 (item destroyed), FALSE otherwise.
    /// </summary>
    public bool DegradeEquipment(int amount)
    {
        CurrentDurability = Mathf.Max(0, CurrentDurability - amount);
        return CurrentDurability <= 0;
    }
}

public enum EquipmentTrait
{
    XPBoost,
    GlobalDamageReduction,
    HealAfterFirstDamage,
    AttackMultiplier,
    CounterChance,
    MoveFirst,
    SoulSteal,
    AllStats,
    SpellReflect,
    MaxHP,
    DoubleStrike,
    ComboStrike,
    TrueStrike
}
