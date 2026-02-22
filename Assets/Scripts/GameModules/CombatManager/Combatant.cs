using System;
using UnityEngine;

public abstract class Combatant : Subject
{
    protected static ActionFactory ActionFactory = new ActionFactory();
    protected Combatant _currentTarget = null;

    public CombatAction CurrentAction = null;
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
}
