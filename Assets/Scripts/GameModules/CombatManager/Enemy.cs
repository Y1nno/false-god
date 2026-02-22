using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Combatant
{
    public string Name { get; protected set; }
    public Resource Health { get; protected set; }
    public Resource Mana { get; protected set; }
    public EnemyStatBox Stats { get; protected set; }

    protected List<CombatAction> _availableActions = new List<CombatAction>();

    public Enemy(string name = "No Name", Dictionary<Stat, int> initialStats = null, int initialHealth = 1, int initialMana = 1, List<int> availableActionIDs = null)
    {
        Name = name;
        if (initialStats == null) initialStats = new Dictionary<Stat, int>()
        {
            { Stat.STR, 1 },
            { Stat.DEX, 1 },
            { Stat.INT, 1 },
            { Stat.SPD, 1 }
        };
        Stats = new EnemyStatBox(initialStats);
        Health = new Resource(initialHealth);
        Mana = new Resource(initialMana);
        if (availableActionIDs == null)
        {
            _availableActions = new List<CombatAction>();
        }
        else
        {
            foreach (int actionID in availableActionIDs)
            {
                CombatAction action = ActionFactory.CreateActionByID(actionID);
                _availableActions.Add(action);
            }
        }
    }

    public override string GetName()
    {
        return Name;
    }
    public override Resource GetHealth()
    {
        return Health;
    }

    public override Resource GetMana()
    {
        return Mana;
    }

    public override void Die()
    {
        TextOutputter.Instance.OutputText($"Enemy {Name} has been defeated!");
        Notify(EventType.EnemyDefeated);
    }

    public override void ChooseAction()
    {
        CombatAction selectedAction = ActionFactory.CreateActionByID(000); // "Do Nothing" action
        int randomIndx = UnityEngine.Random.Range(0, _availableActions.Count);
        if (_availableActions.Count == 0)
        {
            CurrentAction = selectedAction;
            return;
        }
        CombatAction possibleAction = _availableActions[randomIndx];
        //Debug.Log($"Enemy {Name}: actions={_availableActions?.Count ?? -1}, idx={randomIndx}, actionNull={_availableActions[randomIndx] == null}");
        if (possibleAction.CanUse(this))
        {
            selectedAction = possibleAction;
        }
        else
        {
            ChooseAction();
            return;
        }
        CurrentAction = selectedAction;
    }

    public override bool IsAlive()
    {
        return Health.CurrentValue > 0;
    }

    public override int GetStat(Stat stat)
    {
        return Stats.GetStat(stat);
    }

    public override int GetSecondaryStat(SecondaryStat stat)
    {
        return Stats.GetSecondaryStat(stat);
    }

    public override float GetCritChance()
    {
        return Stats.GetSecondaryStat(SecondaryStat.CRIT);
    }

    public override void ExecuteAction()
    {
        if (CurrentAction != null)
        {
            CurrentAction.Execute(this, DecideTarget());
        }
    }

    public virtual Combatant DecideTarget()
    {
        switch (CurrentAction.TargetType)
        {
            case TargetingType.Self:
                return this;
            case TargetingType.SingleEnemy:
                CombatManager cbm  = RunManager.Instance.GetService<CombatManager>();
                return cbm.CurrentBattle.Pcm;
            default:
                return null;
        }
    }

    public override void GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null)
    {
        switch (attackType)
        {
            case AttackType.Physical:
                TakeDamage(damage); // TODO: Apply physical defense
                break;
            case AttackType.Special:
                TakeDamage(damage); // TODO: Apply special defense
                break;
            default:
                TakeDamage(damage);
                break;
        }
    }
}