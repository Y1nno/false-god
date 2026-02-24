using UnityEngine;
using System.Collections.Generic;

public class Equipment : Item
{
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
