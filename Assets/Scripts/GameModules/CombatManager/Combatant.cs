using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Combatant : Subject
{
    protected static ActionFactory ActionFactory = new ActionFactory();
    protected Combatant _currentTarget = null;

    public CombatAction CurrentAction = null;

    // Tracking active effects like HealOverTime or ManaOverTime
    protected List<ActiveOverTimeEffect> _activeEffects = new List<ActiveOverTimeEffect>();

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
    public abstract float GetCritChance();
    public Combatant()
    {
    }
    protected virtual int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;

        bool wasAlive = GetHealth().CurrentValue > 0;
        GetHealth().Decrease(amount);
        TextOutputter.Instance.OutputText($"{GetName()} took {amount} damage.");

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

    public void ApplyAilment(AilmentType type, int duration)
    {
        // Check if we already have this ailment. If so, just refresh/extend the duration based on what's longer, or overwrite.
        // For simplicity, we'll overwrite it if the new duration is longer, or just reset it.
        Ailment existing = ActiveAilments.Find(a => a.Type == type);
        if (existing != null)
        {
            if (duration > existing.RoundsRemaining)
            {
                existing.RoundsRemaining = duration;
                existing.InitialDuration = duration;
            }
            TextOutputter.Instance.OutputText($"{GetName()}'s {type} ailment was refreshed.");
        }
        else
        {
            ActiveAilments.Add(new Ailment(type, duration));
            TextOutputter.Instance.OutputText($"{GetName()} is now afflicted with {type} for {duration} rounds!");
        }
    }

    public void RemoveAilment(AilmentType type)
    {
        int removedCount = ActiveAilments.RemoveAll(a => a.Type == type);
        if (removedCount > 0)
        {
            TextOutputter.Instance.OutputText($"{GetName()} is no longer afflicted with {type}.");
        }
    }

    public bool HasAilment(AilmentType type)
    {
        return ActiveAilments.Exists(a => a.Type == type);
    }

    public virtual void OnRoundEnd()
    {
        if (!IsAlive()) return;

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
            effect.DecrementDuration();
            if (effect.RoundsRemaining <= 0)
            {
                _activeEffects.RemoveAt(i);
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
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * 0.02f);
                        break;
                    case AilmentType.Poison:
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * 0.05f);
                        break;
                    case AilmentType.Bleed:
                        // Bleed increases over time: 2% base + 1% per turn active. Max 8%.
                        int turnsActive = ailment.InitialDuration - ailment.RoundsRemaining;
                        float percentDamage = 0.02f + (0.01f * turnsActive);
                        if (percentDamage > 0.08f) percentDamage = 0.08f;
                        damage = Mathf.RoundToInt(GetHealth().MaxValue * percentDamage);
                        break;
                    case AilmentType.Frozen:
                        
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
