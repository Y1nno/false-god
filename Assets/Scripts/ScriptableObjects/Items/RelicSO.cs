using UnityEngine;

public enum RelicEffectType
{
    GoldBonus,           // Pilgrim's Bone (+10% gold)
    DamageBonus,         // Idol of Endless Hunger (+5% dmg), Hoarder's Fetish (+1% per relic), etc.
    HpLossPerTurn,       // Idol of Endless Hunger (Lose 1 HP every turn)
    CritDamageBonus,     // Crown of the Damned (+20% crit dmg)
    EnemiesDealExtra,    // Crown of the Damned (Enemies deal +10% dmg)
    MaxHpBonus,          // Old King's Stone (+15 Max HP)
    FlatDamageReduction, // Fossilized Heart (-2 dmg on all attacks)
    DamageVsBosses,      // Blasphemer's Reliquary (+15% dmg vs bosses)
    DamagePerTurnSpent,   // Relic of the Thousand Duels (+1 dmg/turn in combat)
    ShopPriceReduction,  // Merchant's Finger Bone (Shop prices -15%)
    SplashDamage,        // Shattered Halo (20% or 30% splash)
    TurnRefreshOnKill,   // Relic of Eternal Hunt (Refill turn on kill)
    UniqueEnemyChance,   // Whispering Reliquary (Higher chance for unique)
    FreeManaChance,      // Blessed Cross (30% free mana)
    DamagePerKillCount   // Hunter's Trophy (+1 dmg per 10 enemies killed)
}

[CreateAssetMenu(fileName = "New Relic", menuName = "Scriptable Objects/Items/Relic")]
public class RelicSO : ItemSO
{
    public RelicEffectType EffectType;
    public float EffectValue;
    public Rarity Rarity;
}
