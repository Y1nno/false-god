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
        // Execute Blocking system before applying defense-mitigated damage
        if (amount > 0 && this is PlayerCombatManager pcm && RunManager.Instance.GetService<EquipmentManager>() is EquipmentManager eqm)
        {
            int blockChance = eqm.GetTotalBlockChance();
            if (blockChance > 0 && UnityEngine.Random.Range(0, 100) < blockChance)
            {
                int blockAmt = eqm.GetTotalBlockAmount();
                amount -= blockAmt;
                TextOutputter.Instance.OutputText($"{GetName()} blocked the attack! Mitigated {blockAmt} damage.");
            }
        }

        if (amount <= 0) 
        {
            TextOutputter.Instance.OutputText($"{GetName()} blocked all incoming damage!");
            return 0;
        }

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
