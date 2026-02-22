using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class Consumable : ScriptableObject
{
    public string ItemName;
    public string Description;

    public List<ConsumableEffect> Effects = new List<ConsumableEffect>();

    // This makes it compatible if you refactor your Item class to use SOs later, 
    // but right now Item is a plain class, so we use this separately or aggregate it.
    public virtual void Use(Combatant target, int tier = 1)
    {
        TextOutputter.Instance.OutputText($"Used {ItemName}...");

        if (ItemName == "Empty Bottle" || ItemName == "EmptyBottle")
        {
            // TODO: Can fill from Fountain give HP or Mana
        }

        bool isPlayer = target.GetName() == "Player"; // Basic way to check based on your Combatant code
        float gluttonyMultiplier = 1.0f;

        if (isPlayer)
        {
            RiteManager rm = RunManager.Instance.GetService<RiteManager>();
            if (rm != null && rm.HasRite<GluttonyRite>())
            {
               gluttonyMultiplier = ((GluttonyRite)rm.GetRite(RiteType.Gluttony)).Multiplier;
            }
        }

        foreach (var effect in Effects)
        {
            ApplyEffect(effect, target, gluttonyMultiplier, tier);
        }
    }

    private void ApplyEffect(ConsumableEffect effect, Combatant target, float multiplier, int tier)
    {
        // For array compatibility: Tier 1 accesses index 0, Tier 2 accesses index 1, Tier 3 accesses index 2.
        // If the array isn't long enough, just fallback to index 0 (the base amount).
        int tierIndex = Mathf.Clamp(tier - 1, 0, Mathf.Max(0, effect.Amount.Length - 1));
        float baseAmount = effect.Amount != null && effect.Amount.Length > 0 ? effect.Amount[tierIndex] : 0;
        float modifiedAmount = baseAmount * multiplier;

        switch (effect.Type)
        {
            case ConsumableEffectType.FlatHP:
                if (modifiedAmount > 0) target.Heal(Mathf.RoundToInt(modifiedAmount));
                else target.TakeConsumableDamage(Mathf.RoundToInt(-modifiedAmount));
                break;

            case ConsumableEffectType.PercentHP:
                // Calculate percentage based on Max Value
                int hpAmount = Mathf.RoundToInt((modifiedAmount / 100f) * target.GetHealth().MaxValue);
                if (hpAmount > 0) target.Heal(hpAmount);
                else target.TakeConsumableDamage(-hpAmount);
                break;

            case ConsumableEffectType.FlatMana:
                if (modifiedAmount > 0) target.RestoreMana(Mathf.RoundToInt(modifiedAmount));
                else target.TryUseMana(Mathf.RoundToInt(-modifiedAmount));
                break;

            case ConsumableEffectType.PercentMana:
                int manaAmount = Mathf.RoundToInt((modifiedAmount / 100f) * target.GetMana().MaxValue);
                if (manaAmount > 0) target.RestoreMana(manaAmount);
                else target.TryUseMana(-manaAmount);
                break;

            case ConsumableEffectType.StatChange:
                // For now, PlayerManager handles stats.
                PlayerManager pmSC = RunManager.Instance.GetService<PlayerManager>();
                if (pmSC != null)
                {
                    if (modifiedAmount > 0) pmSC.IncreaseStat(effect.TargetStat, Mathf.RoundToInt(modifiedAmount));
                    else pmSC.DecreaseStat(effect.TargetStat, Mathf.RoundToInt(-modifiedAmount));
                }
                break;

            case ConsumableEffectType.AllStatsChange:
                PlayerManager pmAll = RunManager.Instance.GetService<PlayerManager>();
                if (pmAll != null)
                {
                    Stat[] allStats = (Stat[])System.Enum.GetValues(typeof(Stat));
                    foreach (Stat stat in allStats)
                    {
                        if (modifiedAmount > 0) pmAll.IncreaseStat(stat, Mathf.RoundToInt(modifiedAmount));
                        else pmAll.DecreaseStat(stat, Mathf.RoundToInt(-modifiedAmount));
                    }
                }
                break;

            case ConsumableEffectType.HealOverTime:
            case ConsumableEffectType.ManaOverTime:
                target.AddActiveEffect(new ActiveOverTimeEffect(effect, modifiedAmount));
                break;
        }
    }
}

public class ActiveOverTimeEffect
{
    public ConsumableEffect BaseEffect { get; private set; }
    public float ModifiedAmount { get; private set; }
    public int RoundsRemaining { get; private set; }

    public ActiveOverTimeEffect(ConsumableEffect baseEffect, float modifiedAmount)
    {
        BaseEffect = baseEffect;
        ModifiedAmount = modifiedAmount;
        RoundsRemaining = baseEffect.Duration;
    }

    public void DecrementDuration()
    {
        RoundsRemaining--;
    }
}
