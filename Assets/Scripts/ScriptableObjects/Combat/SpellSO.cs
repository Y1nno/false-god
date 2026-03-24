using UnityEngine;

public enum SpellEffect { None, Burn, Poison, Freeze, Weaken, Lifesteal, Cleanse, Invulnerable, StatBuff }

[CreateAssetMenu(fileName = "New Spell", menuName = "Scriptable Objects/Combat/Spell")]
public class SpellSO : CombatActionSO
{
    public Rarity Rarity;
    public SpellEffect Effect;
    public int EffectChance; // 0-100
    public int Duration; // for ailments or buffs
    public AttackType Type = AttackType.Special;
    public TargetingType Targeting = TargetingType.SingleEnemy;
    
    [Header("Multi-hit")]
    public int MinHits = 1;
    public int MaxHits = 1;

    [Header("Buffs/Special")]
    public Stat TargetStat;
    public int StatChangePercent;
    public float HealingPercent;
}
