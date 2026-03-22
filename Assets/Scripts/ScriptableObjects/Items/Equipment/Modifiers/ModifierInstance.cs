using UnityEngine;

public enum ModifierEffectType
{
    FlatStat,
    PercentStat,
    Trait
}

public enum EquipmentStat
{
    PhysicalAttack, SpecialAttack, PhysicalDefense, SpecialDefense,
    STR, DEX, INT, SPD, CritChance, BlockChance, BlockAmount, DodgeChance,
    MaxHP, MaxMana
}

[System.Serializable]
public class ModifierInstance
{
    public string Name;
    public ModifierEffectType EffectType;
    public EquipmentStat TargetStat; 
    public int Value;
    public EquipmentTrait Trait;
    
    public CooldownType CooldownType;
    public int CooldownDuration;

    public ModifierInstance(string name, ModifierEffectType type, int value, EquipmentStat stat = EquipmentStat.STR)
    {
        Name = name;
        EffectType = type;
        Value = value;
        TargetStat = stat;
    }

    public ModifierInstance(string name, EquipmentTrait trait, int value = 0)
    {
        Name = name;
        EffectType = ModifierEffectType.Trait;
        Trait = trait;
        Value = value;
    }
}
