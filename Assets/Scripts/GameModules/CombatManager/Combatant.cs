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

    public abstract void ChooseAction();
    public abstract void ExecuteAction();
    public abstract bool IsAlive();
    public abstract int GetStat(Stat stat);
    public abstract int GetSecondaryStat(SecondaryStat stat);
    public abstract Resource GetHealth();
    public abstract Resource GetMana();
    public abstract string GetName();
    public abstract void Die();

    public abstract void GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null);
    public abstract float GetCritChance();
    public Combatant()
    {
    }
    protected virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        GetHealth().Decrease(amount);
        TextOutputter.Instance.OutputText($"{GetName()} took {amount} damage.");
        if (GetHealth().CurrentValue <= 0)
        {
            Die();
        }
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

    public virtual void OnRoundEnd()
    {
        if (_activeEffects.Count == 0 || !IsAlive()) return;

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
}
