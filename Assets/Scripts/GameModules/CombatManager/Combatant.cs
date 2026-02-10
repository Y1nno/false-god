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
    public abstract Resource GetHealth();
    public abstract Resource GetMana();
    public abstract string GetName();
    public abstract void Die();
    public Combatant()
    {
    }
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        GetHealth().Decrease(amount);
        TextOutputter.Instance.OutputText($"{GetName()} took {amount} damage.");
        if (GetHealth().CurrentValue <= 0)
        {
            Die();
        }
    }

    public bool TryUseMana(int amount)
    {
        if (!GetMana().CanAfford(amount))
        {
            return false;
        }
        GetMana().Decrease(amount);
        return true;
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
