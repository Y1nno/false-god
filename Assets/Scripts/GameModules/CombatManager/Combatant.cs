using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Combatant : Subject
{
    protected static ActionFactory ActionFactory = new ActionFactory();
    protected Combatant _currentTarget = null;
    protected int _goldValue;

    public bool IsBoss { get; set; } = false;
    public int GoldValue => _goldValue;
    public int BaseXP { get; protected set; }
    public int Level { get; protected set; } = 1;
    public CombatAction CurrentAction = null;
    public int CurrentBlock { get; set; } = 0;

    // Tracking active effects like HealOverTime or ManaOverTime
    protected List<ActiveOverTimeEffect> _activeEffects = new List<ActiveOverTimeEffect>();

    public void ClearEncounterEffects()
    {
        if (_activeEffects.Count == 0) return;

        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            if (effect.BaseEffect.DurationType == EffectDurationType.Encounter)
            {
                effect.DecrementDuration();
                if (effect.RoundsRemaining <= 0)
                {
                    _activeEffects.RemoveAt(i);
                    TextOutputter.Instance.OutputText($"{GetName()}'s encounter effect ({effect.BaseEffect.Type}) has expired.");
                }
            }
        }
    }

    // Tracking active ailments (Burn, Poison, Frozen, Bleed, etc)
    public List<Ailment> ActiveAilments = new List<Ailment>();

    public abstract void ChooseAction();
    public abstract void ExecuteAction();
    public abstract bool IsAlive();
    public abstract int GetStat(Stat stat);
    public abstract int GetSecondaryStat(SecondaryStat stat);
    public abstract Resource GetHealth();
    public abstract Resource GetMana();
    public virtual int GetBonusDamage() { return 0; }
    public abstract string GetName();
    public abstract void Die();

    public abstract int GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null);
    public virtual void OnDealDamage(int damage, Combatant target) { }
    public abstract float GetCritChance();
    protected virtual int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;

        bool wasAlive = GetHealth().CurrentValue > 0;
        int originalAmount = amount;
        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        if (relicm != null)
        {
            amount = Mathf.Max(0, amount - relicm.GetFlatDamageReduction());
        }

        // Apply Block/Shield
        int damageRemaining = amount;
        int blockMitigated = 0;
        if (CurrentBlock > 0)
        {
            blockMitigated = Mathf.Min(CurrentBlock, damageRemaining);
            CurrentBlock -= blockMitigated;
            damageRemaining -= blockMitigated;
            TextOutputter.Instance.OutputText($"{GetName()}'s block absorbed {blockMitigated} damage! ({CurrentBlock} Block remaining)");
        }

        if (damageRemaining <= 0) 
        {
            return amount - originalAmount; // All damage blocked
        }

        GetHealth().Decrease(damageRemaining);
        string mitigationLog = originalAmount != amount ? $" (Mitigated {originalAmount - amount} from Relics)" : "";
        TextOutputter.Instance.OutputText($"{GetName()} took {damageRemaining} damage from the attack.{mitigationLog}");

        if (wasAlive && GetHealth().CurrentValue <= 0)
        {
            Die();
        }
        return amount;
    }

    public virtual bool TryUseMana(int amount)
    {
        if (!GetMana().CanAfford(amount))
        {
            return false;
        }
        GetMana().Decrease(amount);
        return true;
    }

    public virtual bool CanAffordMana(int amount)
    {
        return GetMana().CanAfford(amount);
    }

    public void TakeConsumableDamage(int amount)
    {
        if (amount <= 0) return;
        TakeDamage(amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        
        // Bleed Logic: Reduce healing based on stacks
        Ailment bleed = ActiveAilments.Find(a => a.Type == AilmentType.Bleed);
        if (bleed != null)
        {
            float reduction = AilmentScaling.GetStatModifier(AilmentType.Bleed, bleed.Stacks);
            int reducedAmount = Mathf.RoundToInt(amount * (1f - reduction));
            TextOutputter.Instance.OutputText($"{GetName()}'s healing was reduced by {Mathf.RoundToInt(reduction * 100)}% due to Bleed!");
            amount = reducedAmount;
        }

        GetHealth().Increase(amount);
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0) return;
        GetMana().Increase(amount);
    }

    public void AddActiveEffect(ActiveOverTimeEffect effect)
    {
        _activeEffects.Add(effect);
        TextOutputter.Instance.OutputText($"{GetName()} gained an over-time effect ({effect.BaseEffect.Type}) for {effect.RoundsRemaining} turns.");
    }

    public void ApplyAilment(AilmentType type, int _unusedValue = 0)
    {
        TryApplyAilment(type, 1, 100f);
    }

    public void TryApplyAilment(AilmentType type, int stacksToAdd = 1, float baseChance = 100f)
    {
        // Whispering Flame Lvl 1: Burn Immunity
        if (type == AilmentType.Burn && this is PlayerCombatManager)
        {
            ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
            if (rm?.CurrentReligion is WhisperingFlame && rm.CurrentReligion.CurrentFaithLevel >= 1)
            {
                TextOutputter.Instance.OutputText("Burn Immunity: The flame cannot hurt a child of the Coven.");
                return;
            }
        }

        Ailment existing = ActiveAilments.Find(a => a.Type == type);
        int currentStacks = existing?.Stacks ?? 0;

        // Chance of afflicting ailments reduce 20% per stack
        float finalChance = baseChance * (1f - (0.2f * currentStacks));
        if (UnityEngine.Random.Range(0f, 100f) > finalChance)
        {
            if (currentStacks > 0) TextOutputter.Instance.OutputText($"{GetName()} resisted the {type} stack (Chance: {Mathf.RoundToInt(finalChance)}%).");
            return;
        }

        if (existing != null)
        {
            existing.Stacks = Mathf.Min(5, existing.Stacks + stacksToAdd);
            int newDuration = AilmentScaling.GetDuration(type, existing.Stacks);
            existing.RoundsRemaining = newDuration;
            existing.InitialDuration = newDuration;
            TextOutputter.Instance.OutputText($"{GetName()}'s {type} increased to {existing.Stacks} stacks! (Duration: {newDuration} rounds)");
        }
        else
        {
            int duration = AilmentScaling.GetDuration(type, stacksToAdd);
            ActiveAilments.Add(new Ailment(type, duration, stacksToAdd));
            TextOutputter.Instance.OutputText($"{GetName()} is now afflicted with {type} for {duration} rounds!");
        }
    }

    public void RemoveAilment(AilmentType type)
    {
        ActiveAilments.RemoveAll(a => a.Type == type);
    }

    public List<SerializableActiveEffect> GetActiveEffects()
    {
        var list = new List<SerializableActiveEffect>();
        if (_activeEffects == null) return list;
        foreach (var effect in _activeEffects)
        {
            list.Add(new SerializableActiveEffect
            {
                EffectType = effect.BaseEffect.Type.ToString(),
                ModifiedAmount = effect.ModifiedAmount,
                RoundsRemaining = effect.RoundsRemaining,
                TargetStat = effect.BaseEffect.TargetStat.ToString(),
                DurationType = effect.BaseEffect.DurationType.ToString()
            });
        }
        return list;
    }

    public void RestoreActiveEffects(List<SerializableActiveEffect> savedEffects)
    {
        if (savedEffects == null) return;
        _activeEffects.Clear();
        foreach (var s in savedEffects)
        {
            if (Enum.TryParse(s.EffectType, out ConsumableEffectType type) &&
                Enum.TryParse(s.TargetStat, out Stat tStat) &&
                Enum.TryParse(s.DurationType, out EffectDurationType dType))
            {
                var baseEffect = new ConsumableEffect { Type = type, TargetStat = tStat, DurationType = dType };
                _activeEffects.Add(new ActiveOverTimeEffect(baseEffect, s.ModifiedAmount) { RoundsRemaining = s.RoundsRemaining });
            }
        }
    }

    public void RestoreAilments(List<SerializableAilment> savedAilments)
    {
        ActiveAilments.Clear();
        foreach (var s in savedAilments)
        {
            if (Enum.TryParse(s.AilmentType, out AilmentType type))
            {
                ActiveAilments.Add(new Ailment(type, s.Duration, s.Stacks));
            }
        }
    }

    public bool HasAilment(AilmentType type)
    {
        return ActiveAilments.Exists(a => a.Type == type);
    }

    public virtual void OnRoundEnd()
    {
        if (!IsAlive()) return;

        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        if (relicm != null && this is PlayerCombatManager)
        {
            int hpLoss = relicm.GetHpLossPerTurn();
            if (hpLoss > 0)
            {
                TextOutputter.Instance.OutputText($"{GetName()} loses {hpLoss} HP from the Idol of Endless Hunger.");
                TakeDamage(hpLoss);
            }
        }

        if (_activeEffects.Count > 0)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveOverTimeEffect effect = _activeEffects[i];

            // Apply effect
            if (effect.BaseEffect.Type == ConsumableEffectType.HealOverTime)
            {
                int healAmount = Mathf.RoundToInt((effect.ModifiedAmount / 100f) * GetHealth().MaxValue);
                Heal(healAmount);
                TextOutputter.Instance.OutputText($"{GetName()} regenerates {healAmount} HP from {effect.BaseEffect.Type}.");
            }
            else if (effect.BaseEffect.Type == ConsumableEffectType.ManaOverTime)
            {
                int manaAmount = Mathf.RoundToInt((effect.ModifiedAmount / 100f) * GetMana().MaxValue);
                RestoreMana(manaAmount);
                TextOutputter.Instance.OutputText($"{GetName()} regenerates {manaAmount} Mana from {effect.BaseEffect.Type}.");
            }

        // Decrement and remove if finished
        if (effect.BaseEffect.DurationType == EffectDurationType.Turns)
        {
            effect.DecrementDuration();
            if (effect.RoundsRemaining <= 0)
            {
                _activeEffects.RemoveAt(i);
            }
        }
            }
        }

        // Process ailments
        if (ActiveAilments.Count > 0 && IsAlive())
        {
            for (int i = ActiveAilments.Count - 1; i >= 0; i--)
            {
                Ailment ailment = ActiveAilments[i];
                int damage = 0;

                switch (ailment.Type)
                {
                    case AilmentType.Burn:
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * AilmentScaling.GetDamagePercent(AilmentType.Burn, ailment.Stacks));
                        break;
                    case AilmentType.Poison:
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * AilmentScaling.GetDamagePercent(AilmentType.Poison, ailment.Stacks));
                        break;
                    case AilmentType.Bleed:
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * AilmentScaling.GetDamagePercent(AilmentType.Bleed, ailment.Stacks));
                        break;
                    case AilmentType.Frozen:
                        // Frozen only prevents move, handled in ChooseAction
                        break;
                }

                if (damage > 0)
                {
                    TextOutputter.Instance.OutputText($"{GetName()} took {damage} damage from {ailment.Type}.");
                    TakeDamage(damage);
                }

                // If combatant died from DoT, exit early
                if (!IsAlive()) return;

                ailment.DecrementDuration();
                if (ailment.RoundsRemaining <= 0)
                {
                    TextOutputter.Instance.OutputText($"{GetName()} recovered from {ailment.Type}.");
                    ActiveAilments.RemoveAt(i);
                }
            }
        }
    }
}
