using UnityEngine;
using System.Collections.Generic;

public class Equipment : Item
{
    public EquipmentSO BaseData { get; private set; }
    public string ItemName { get; private set; }
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

        Traits = baseData.Traits != null ? new List<TraitWithValue>(baseData.Traits) : new List<TraitWithValue>();
        
        // Note: We leave Skill null for runtime resolution if we aren't instantiating combat actions here directly.
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
