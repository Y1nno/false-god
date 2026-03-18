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
    protected int _goldValue = 0;
    public int GoldValue => _goldValue;

    public Enemy(EnemySO data, int level)
    {
        IsBoss = false;
        Name = data.EnemyName;
        
        float l_factor = level - 1;
        int maxHp = Mathf.RoundToInt(data.BaseHealth * (1 + l_factor * 0.25f));
        Health = new Resource(maxHp);
        
        Mana = new Resource(data.BaseMana);
        
        int atk = Mathf.RoundToInt(data.BaseAtk * (1 + l_factor * 0.18f));
        int spAtk = Mathf.RoundToInt(data.BaseSpAtk * (1 + l_factor * 0.18f));
        int def = Mathf.RoundToInt(data.BaseDef * (1 + l_factor * 0.12f));
        int spDef = Mathf.RoundToInt(data.BaseSpDef * (1 + l_factor * 0.12f));
        
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int dungeonFloor = dm != null ? dm.CurrentDungeonFloor : 1;
        int speed = data.BaseSpeed + Mathf.FloorToInt(dungeonFloor * l_factor * 0.4f);

        Dictionary<Stat, int> primStats = new Dictionary<Stat, int>
        {
            { Stat.STR, atk },
            { Stat.DEX, 10 },    // Base placeholder if strictly required
            { Stat.INT, spAtk },
            { Stat.SPD, speed }
        };

        Dictionary<SecondaryStat, int> secStats = new Dictionary<SecondaryStat, int>
        {
            { SecondaryStat.PHATK, atk },
            { SecondaryStat.SPATK, spAtk },
            { SecondaryStat.PHDEF, def },
            { SecondaryStat.SPDEF, spDef },
            { SecondaryStat.CRIT, Mathf.RoundToInt(data.CritChance) },
            { SecondaryStat.EVDE, Mathf.RoundToInt(data.DodgeChance) }
        };
        
        Stats = new EnemyStatBox(primStats, secStats);

        if (data.AvailableActionIDs != null)
        {
            foreach (int actionID in data.AvailableActionIDs)
            {
                CombatAction action = ActionFactory.CreateActionByID(actionID);
                if (action != null) _availableActions.Add(action);
            }
        }
        
        _goldValue = data.GoldValue;
        
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (em != null) AttachObserver(em);
        
        DropManager dropm = RunManager.Instance.GetService<DropManager>();
        if (dropm != null) AttachObserver(dropm);
    }

    public Enemy(string name = "No Name", Dictionary<Stat, int> initialStats = null, int initialHealth = 1, int initialMana = 1, List<int> availableActionIDs = null, int goldValue = 0, bool isBoss = false)
    {
        IsBoss = isBoss;
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
                if (action != null)
                {
                    _availableActions.Add(action);
                }
                else
                {
                    Debug.LogWarning($"Enemy {name}: Action ID {actionID} returned null from ActionFactory.");
                }
            }
        }
        _goldValue = goldValue;

        AttachObserver(RunManager.Instance.GetService<EconomyManager>());
        AttachObserver(RunManager.Instance.GetService<DropManager>());
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
        if (HasAilment(AilmentType.Frozen))
        {
            CurrentAction = ActionFactory.CreateActionByID(000); // Do Nothing
            TextOutputter.Instance.OutputText($"{Name} is frozen solid and cannot move!");
            return;
        }

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
        int baseStat = Stats.GetSecondaryStat(stat);

        if (HasAilment(AilmentType.Burn) && stat == SecondaryStat.PHATK)
        {
            baseStat = Mathf.RoundToInt(baseStat * 0.90f); // Reduces attack damage by 10%
        }
        else if (HasAilment(AilmentType.Poison) && stat == SecondaryStat.SPDEF)
        {
            baseStat = Mathf.RoundToInt(baseStat * 0.95f); // Reduces Sp.Def by 5%
        }

        if (HasAilment(AilmentType.Frozen))
        {
            if (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF)
            {
                baseStat = Mathf.RoundToInt(baseStat * 1.20f); // +20% Def and Sp.Def
            }
        }

        if (HasAilment(AilmentType.AtkBonus) && (stat == SecondaryStat.PHATK || stat == SecondaryStat.SPATK))
        {
            baseStat = Mathf.RoundToInt(baseStat * 1.40f); // +40% Attack
        }

        if (HasAilment(AilmentType.Shielded) && (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF))
        {
            baseStat = Mathf.RoundToInt(baseStat * 1.20f); // +20% Defense
        }

        return baseStat;
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

    public override int GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null)
    {
        bool hasTrueStrike = false;
        if (attacker is PlayerCombatManager)
        {
            EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
            if (eqm != null && eqm.HasTrait(EquipmentTrait.TrueStrike))
            {
                hasTrueStrike = true;
                TextOutputter.Instance.OutputText($"{attacker.GetName()}'s TrueStrike ignores defense!");
            }
        }

        switch (attackType)
        {
            case AttackType.Physical:
                return TakeDamage(hasTrueStrike ? damage : damage - GetSecondaryStat(SecondaryStat.PHDEF));
            case AttackType.Special:
                return TakeDamage(hasTrueStrike ? damage : damage - GetSecondaryStat(SecondaryStat.SPDEF));
            default:
                return TakeDamage(damage);
        }
    }
}