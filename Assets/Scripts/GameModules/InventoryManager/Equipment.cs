using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum MainAttribute { None, Strength, Dexterity, Intelligence }

public class Equipment : Item
{
    public EquipmentSO BaseData { get; private set; }
    public string BaseItemName { get; private set; }
    
    public List<ModifierInstance> Prefixes { get; private set; } = new List<ModifierInstance>();
    public List<ModifierInstance> Suffixes { get; private set; } = new List<ModifierInstance>();

    public override string GetName()
    {
        string prefixStr = string.Join(" ", Prefixes.Select(p => p.Name));
        string suffixStr = Suffixes.Count > 0 ? "of " + string.Join("-", Suffixes.Select(s => s.Name)) : "";
        
        string fullName = BaseItemName;
        if (!string.IsNullOrEmpty(prefixStr)) fullName = prefixStr + " " + fullName;
        if (!string.IsNullOrEmpty(suffixStr)) fullName = fullName + " " + suffixStr;
        
        return fullName;
    }

    public bool IsCatalyst => ScalingStat == MainAttribute.Intelligence && Slot == EquipmentSlot.Weapon;

    public EquipmentSlot Slot { get; private set; }
    public Rarity Rarity { get; private set; }
    public new bool IsTwoHanded { get; private set; }

    private int _basePhysicalDefense;
    private int _baseSpecialDefense;
    private int _baseSpecialAttack;
    private int _basePhysicalAttack;
    private int _baseSTR;
    private int _baseDEX;
    private int _baseINT;
    private int _baseSPD;
    private int _baseCritChance;
    private int _baseBlockChance;
    private int _baseBlockAmount;
    private int _baseDodgeChance;
    private int _baseMaxHP;
    private int _baseMaxMana;

    public int PhysicalDefense => CalculateStat(EquipmentStat.PhysicalDefense, _basePhysicalDefense);
    public int SpecialDefense => CalculateStat(EquipmentStat.SpecialDefense, _baseSpecialDefense);
    public int PhysicalAttack => CalculateStat(EquipmentStat.PhysicalAttack, _basePhysicalAttack);
    public int SpecialAttack => CalculateStat(EquipmentStat.SpecialAttack, _baseSpecialAttack);
    public int STR => CalculateStat(EquipmentStat.STR, _baseSTR);
    public int DEX => CalculateStat(EquipmentStat.DEX, _baseDEX);
    public int INT => CalculateStat(EquipmentStat.INT, _baseINT);
    public int SPD => CalculateStat(EquipmentStat.SPD, _baseSPD);
    public int CritChance => CalculateStat(EquipmentStat.CritChance, _baseCritChance);
    public int BlockChance => CalculateStat(EquipmentStat.BlockChance, _baseBlockChance);
    public int BlockAmount => CalculateStat(EquipmentStat.BlockAmount, _baseBlockAmount);
    public int DodgeChance => CalculateStat(EquipmentStat.DodgeChance, _baseDodgeChance);
    public int MaxHP => CalculateStat(EquipmentStat.MaxHP, _baseMaxHP);
    public int MaxMana => CalculateStat(EquipmentStat.MaxMana, _baseMaxMana);
    
    // Durability System
    public int MaxDurability { get; private set; }
    public int CurrentDurability { get; private set; }

    public List<TraitWithValue> Traits { get; private set; }
    public CombatAction Skill { get; private set; }
    public MainAttribute ScalingStat { get; private set; }

    public Equipment(EquipmentSO baseData)
    {
        BaseData = baseData;
        BaseItemName = baseData.ItemName;
        Slot = baseData.Slot;
        Rarity = baseData.Rarity;
        IsTwoHanded = baseData.IsTwoHanded;
        ScalingStat = baseData.ScalingStat;

        _basePhysicalDefense = baseData.PhysicalDefense.value != 0 ? baseData.PhysicalDefense.value : baseData.PhysicalDefense.max;
        _baseSpecialDefense = baseData.SpecialDefense.value != 0 ? baseData.SpecialDefense.value : baseData.SpecialDefense.max;
        _basePhysicalAttack = baseData.PhysicalAttack.value != 0 ? baseData.PhysicalAttack.value : baseData.PhysicalAttack.max;
        _baseSpecialAttack = baseData.SpecialAttack.value != 0 ? baseData.SpecialAttack.value : baseData.SpecialAttack.max;
        _baseSTR = baseData.STR.value != 0 ? baseData.STR.value : baseData.STR.max;
        _baseDEX = baseData.DEX.value != 0 ? baseData.DEX.value : baseData.DEX.max;
        _baseINT = baseData.INT.value != 0 ? baseData.INT.value : baseData.INT.max;
        _baseSPD = baseData.SPD.value != 0 ? baseData.SPD.value : baseData.SPD.max;
        _baseCritChance = baseData.CritChance.value != 0 ? baseData.CritChance.value : baseData.CritChance.max;
        _baseBlockChance = baseData.BlockChance.value != 0 ? baseData.BlockChance.value : baseData.BlockChance.max;
        _baseBlockAmount = baseData.BlockAmount.value != 0 ? baseData.BlockAmount.value : baseData.BlockAmount.max;
        _baseDodgeChance = baseData.DodgeChance.value != 0 ? baseData.DodgeChance.value : baseData.DodgeChance.max;
        // MaxHP and MaxMana can be added to EquipmentSO later if needed, starting at 0 base for now
        _baseMaxHP = 0;
        _baseMaxMana = 0;

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

    private int CalculateStat(EquipmentStat stat, int baseValue)
    {
        int flat = 0;
        float percent = 0;

        foreach (var mod in Prefixes)
        {
            if (mod.EffectType != ModifierEffectType.Trait && mod.TargetStat == stat)
            {
                if (mod.EffectType == ModifierEffectType.FlatStat) flat += mod.Value;
                else if (mod.EffectType == ModifierEffectType.PercentStat) percent += mod.Value / 100.0f;
            }
        }
        foreach (var mod in Suffixes)
        {
            if (mod.EffectType != ModifierEffectType.Trait && mod.TargetStat == stat)
            {
                if (mod.EffectType == ModifierEffectType.FlatStat) flat += mod.Value;
                else if (mod.EffectType == ModifierEffectType.PercentStat) percent += mod.Value / 100.0f;
            }
        }

        return Mathf.RoundToInt((baseValue + flat) * (1.0f + percent));
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
    TrueStrike,
    MagicFind,
    PoisonHit,
    BurnHit,
    FreezeHit,
    WeakenHit,
    HealthRegen,
    ManaRegen,
    DamageReflect,
    AilmentResist,
    AilmentDurationReduction
}
