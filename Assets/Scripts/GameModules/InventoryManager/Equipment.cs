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
    public int UpgradeLevel { get; set; } = 0;

    public override string GetName()
    {
        string prefixStr = string.Join(" ", Prefixes.Select(p => p.Name));
        string suffixStr = Suffixes.Count > 0 ? "of " + string.Join("-", Suffixes.Select(s => s.Name)) : "";
        
        string fullName = BaseItemName;
        if (!string.IsNullOrEmpty(prefixStr)) fullName = prefixStr + " " + fullName;
        if (!string.IsNullOrEmpty(suffixStr)) fullName = fullName + " " + suffixStr;
        if (UpgradeLevel > 0) fullName += " +" + UpgradeLevel;
        
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

    /// <summary>
    /// Restores current durability to its maximum value.
    /// </summary>
    public void Repair()
    {
        CurrentDurability = MaxDurability;
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

        float upgradeBonus = GetUpgradeMultiplier();
        return Mathf.RoundToInt((baseValue + flat) * (1.0f + percent + upgradeBonus));
    }

    private float GetUpgradeMultiplier()
    {
        if (UpgradeLevel <= 0) return 0f;

        // Bonuses from charts supplied by User
        return Rarity switch
        {
            Rarity.Common => UpgradeLevel switch
            {
                1 => 0.10f,
                2 => 0.20f,
                3 => 0.50f,
                _ => 0.50f
            },
            Rarity.Uncommon => UpgradeLevel switch
            {
                1 => 0.10f,
                2 => 0.20f,
                3 => 0.40f,
                4 => 0.65f,
                5 => 1.05f,
                _ => 1.05f
            },
            Rarity.Rare => UpgradeLevel switch
            {
                1 => 0.10f,
                2 => 0.20f,
                3 => 0.35f,
                4 => 0.60f,
                5 => 0.97f,
                6 => 1.47f,
                7 => 2.12f,
                _ => 2.12f
            },
            Rarity.Epic => UpgradeLevel switch
            {
                1 => 0.15f,
                2 => 0.35f,
                3 => 0.65f,
                4 => 1.00f,
                5 => 1.50f,
                6 => 2.10f,
                7 => 2.90f,
                8 => 3.90f,
                9 => 5.15f,
                _ => 5.15f
            },
            Rarity.Legendary => UpgradeLevel switch
            {
                1 => 0.20f,
                2 => 0.50f,
                3 => 0.95f,
                4 => 1.45f,
                5 => 2.05f,
                6 => 2.90f,
                7 => 3.90f,
                8 => 5.15f,
                9 => 6.65f,
                10 => 8.65f,
                _ => 8.65f
            },
            _ => 0f
        };
    }

    public int GetUpgradeCost()
    {
        int nextLevel = UpgradeLevel + 1;
        return Rarity switch
        {
            Rarity.Common => nextLevel switch
            {
                1 => 25, 2 => 30, 3 => 50, _ => 0
            },
            Rarity.Uncommon => nextLevel switch
            {
                1 => 30, 2 => 35, 3 => 50, 4 => 75, 5 => 100, _ => 0
            },
            Rarity.Rare => nextLevel switch
            {
                1 => 50, 2 => 85, 3 => 125, 4 => 175, 5 => 200, 6 => 250, 7 => 350, _ => 0
            },
            Rarity.Epic => nextLevel switch
            {
                1 => 150, 2 => 200, 3 => 250, 4 => 300, 5 => 400, 6 => 550, 7 => 700, 8 => 850, 9 => 1000, _ => 0
            },
            Rarity.Legendary => nextLevel switch
            {
                1 => 250, 2 => 300, 3 => 350, 4 => 400, 5 => 500, 6 => 700, 7 => 850, 8 => 1000, 9 => 1200, 10 => 1500, _ => 0
            },
            _ => 0
        };
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
    // MagicFind,
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
