// TRIGGER RECOMPILE: 1773950000
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPrefix { None, Frenzied, Armored, Swift, Vampiric, Brutal }
public enum EnemySuffix { None, ofVenom, ofFrost, ofEmbers, ofDecay, ofRupture }

public class Enemy : Combatant
{
    public string Name { get; protected set; }
    public EnemySO EnemyData { get; private set; }
    public EnemyPrefix Prefix { get; private set; } = EnemyPrefix.None;
    public EnemySuffix Suffix { get; private set; } = EnemySuffix.None;
    public bool IsUnique => Prefix != EnemyPrefix.None || Suffix != EnemySuffix.None;

    public Resource Health { get; protected set; }
    public Resource Mana { get; protected set; }
    public EnemyStatBox Stats { get; protected set; }

    protected List<CombatAction> _availableActions = new List<CombatAction>();

    public Enemy(EnemySO data, int level)
    {
        EnemyData = data;
        IsBoss = false;
        Name = data.EnemyName;
        BaseXP = data.BaseXP;
        Level = level;
        
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

    public void BecomeUnique(EnemyPrefix prefix, EnemySuffix suffix)
    {
        Prefix = prefix;
        Suffix = suffix;

        // Apply HP modifiers immediately
        if (Prefix == EnemyPrefix.Armored)
        {
            int hpBonus = Mathf.RoundToInt(Health.MaxValue * 0.20f);
            Health.IncreaseBaseMax(hpBonus);
            Health.Increase(hpBonus);
        }
        else if (Prefix == EnemyPrefix.Swift)
        {
            int hpPenalty = Mathf.RoundToInt(Health.MaxValue * 0.20f);
            Health.IncreaseBaseMax(-hpPenalty);
        }

        TextOutputter.Instance.OutputText($"A unique monster appears: {GetName()}!");
    }

    public override string GetName()
    {
        string pStr = Prefix != EnemyPrefix.None ? Prefix.ToString() + " " : "";
        string sStr = Suffix != EnemySuffix.None ? " " + Suffix.ToString().Replace("of", "of ") : "";
        return $"{pStr}{Name}{sStr}";
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
        
        // --- UNIQUE OF EMBERS ON DEATH ---
        if (Suffix == EnemySuffix.ofEmbers)
        {
            CombatManager cm = RunManager.Instance.GetService<CombatManager>();
            if (cm != null && cm.CurrentBattle != null && UnityEngine.Random.value <= 0.55f)
            {
                TextOutputter.Instance.OutputText($"{GetName()} explodes into embers!");
                cm.CurrentBattle.Pcm.ApplyAilment(AilmentType.Burn, 3);
            }
        }

        Notify(EventType.EnemyDefeated);
    }

    public override void ChooseAction()
    {
        if (HasAilment(AilmentType.Frozen) || HasAilment(AilmentType.Stun) || HasAilment(AilmentType.Reloading))
        {
            CurrentAction = ActionFactory.CreateActionByID(000); // Do Nothing
            string reason = HasAilment(AilmentType.Frozen) ? "frozen solid" : (HasAilment(AilmentType.Stun) ? "stunned" : "reloading");
            TextOutputter.Instance.OutputText($"{GetName()} is {reason} and cannot move!");
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
        int baseStat = Stats.GetStat(stat);

        // --- UNIQUE PREFIX STAT MODIFIERS ---
        if (Prefix == EnemyPrefix.Frenzied && stat == Stat.SPD)
        {
            baseStat = Mathf.RoundToInt(baseStat * 1.20f);
        }
        else if (Prefix == EnemyPrefix.Armored && stat == Stat.SPD)
        {
            baseStat = Mathf.RoundToInt(baseStat * 0.80f);
        }
        else if (Prefix == EnemyPrefix.Swift && stat == Stat.SPD)
        {
            baseStat = Mathf.RoundToInt(baseStat * 1.40f);
        }

        return baseStat;
    }

    public override int GetSecondaryStat(SecondaryStat stat)
    {
        int baseStat = Stats.GetSecondaryStat(stat);

        // Apply Ailment Modifiers
        foreach (var ailment in ActiveAilments)
        {
            float mod = AilmentScaling.GetStatModifier(ailment.Type, ailment.Stacks);
            if (mod == 0) continue;

            bool applies = false;
            switch (ailment.Type)
            {
                case AilmentType.Burn:
                    applies = (stat == SecondaryStat.PHATK || stat == SecondaryStat.SPATK);
                    break;
                case AilmentType.Poison:
                    applies = (stat == SecondaryStat.SPDEF);
                    break;
                case AilmentType.Frozen:
                    applies = (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF);
                    break;
                case AilmentType.Weaken:
                    applies = (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF);
                    break;
                case AilmentType.AtkDebuff:
                    applies = (stat == SecondaryStat.PHATK || stat == SecondaryStat.SPATK);
                    break;
                case AilmentType.DefDebuff:
                    applies = (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF);
                    break;
                case AilmentType.AtkBonus:
                    if (stat == SecondaryStat.PHATK || stat == SecondaryStat.SPATK)
                    {
                        baseStat = Mathf.RoundToInt(baseStat * 1.40f);
                    }
                    break;
                case AilmentType.Shielded:
                    if (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF)
                    {
                        baseStat = Mathf.RoundToInt(baseStat * 1.20f);
                    }
                    break;
            }

            if (applies)
            {
                baseStat = Mathf.RoundToInt(baseStat * (1f - mod));
            }
        }

        // --- UNIQUE PREFIX MODIFIERS ---
        if (Prefix == EnemyPrefix.Frenzied)
        {
            if (stat == SecondaryStat.PHATK || stat == SecondaryStat.SPATK) baseStat = Mathf.RoundToInt(baseStat * 1.30f);
            if (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF) baseStat = Mathf.RoundToInt(baseStat * 0.80f);
        }
        else if (Prefix == EnemyPrefix.Armored)
        {
            if (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF) baseStat = Mathf.RoundToInt(baseStat * 1.30f);
        }
        else if (Prefix == EnemyPrefix.Swift)
        {
            if (stat == SecondaryStat.EVDE) baseStat += 15;
        }
        else if (Prefix == EnemyPrefix.Brutal)
        {
            if (stat == SecondaryStat.CRIT) baseStat += 50;
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
            // Skeleton Archer Priority
            if (GetName() == "Skeleton Archer")
            {
                CurrentAction.Priority = 1;
            }

            Combatant target = DecideTarget();
            if (target != null)
            {
                CurrentAction.Execute(this, target);
                
                // On-Hit Effects
                ApplyOnHitEffects(target);

                // Skeleton Archer Reload
                if (GetName() == "Skeleton Archer")
                {
                    ApplyAilment(AilmentType.Reloading, 1);
                }
            }
        }
    }

    protected virtual void ApplyOnHitEffects(Combatant target)
    {
        if (target == null || !target.IsAlive()) return;

        string name = GetName();
        if (name == "Imp")
        {
            target.TryApplyAilment(AilmentType.Bleed, 1, 30f);
        }
        else if (name == "Infected Hound")
        {
            target.TryApplyAilment(AilmentType.Poison, 1, 30f);
        }
        else if (name == "infernal")
        {
            target.TryApplyAilment(AilmentType.Burn, 1, 30f);
        }
        else if (name == "Succubus")
        {
            target.TryApplyAilment(AilmentType.Charmed, 1, 50f);
        }

        // --- UNIQUE VAMPIRIC PREFIX ---
        if (Prefix == EnemyPrefix.Vampiric)
        {
            // Handled via HandleLifeSteal hook in ExecuteAction
        }

        // --- UNIQUE SUFFIX EFFECTS ---
        if (Suffix == EnemySuffix.ofVenom) target.TryApplyAilment(AilmentType.Poison, 1, 30f);
        else if (Suffix == EnemySuffix.ofFrost) target.TryApplyAilment(AilmentType.Frozen, 1, 25f);
        else if (Suffix == EnemySuffix.ofEmbers) target.TryApplyAilment(AilmentType.Burn, 1, 10f);
        else if (Suffix == EnemySuffix.ofDecay) target.TryApplyAilment(AilmentType.DmgDebuff, 1, 25f);
        else if (Suffix == EnemySuffix.ofRupture) target.TryApplyAilment(AilmentType.Bleed, 1, 20f);
    }
    
    // Helper to handle Vampiric heal
    public void HandleLifeSteal(int damageDealt)
    {
        if (Prefix == EnemyPrefix.Vampiric && damageDealt > 0)
        {
            int heal = Mathf.RoundToInt(damageDealt * 0.25f);
            if (heal > 0)
            {
                Health.Increase(heal);
                TextOutputter.Instance.OutputText($"{GetName()} heels {heal} HP from Vampiric drain!");
            }
        }
    }

    public virtual Combatant DecideTarget()
    {
        if (CurrentAction == null) return null;

        switch (CurrentAction.TargetType)
        {
            case TargetingType.Self:
                return this;
            case TargetingType.SingleEnemy:
                // Handle Charmed status: 50% chance to target self/allies instead
                if (HasAilment(AilmentType.Charmed) && UnityEngine.Random.value <= 0.5f)
                {
                    TextOutputter.Instance.OutputText($"{GetName()} is charmed and targets itself!");
                    return this;
                }

                CombatManager cbm  = RunManager.Instance.GetService<CombatManager>();
                if (cbm == null || cbm.CurrentBattle == null) return null;
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

        int damageToTake = damage;
        string name = GetName();

        if (name == "Kobold" && attackType == AttackType.Physical)
        {
            int blocked = Mathf.RoundToInt(damage * 0.70f);
            damageToTake -= blocked;
            TextOutputter.Instance.OutputText($"{name} blocks {blocked} damage (70% passive)!");
        }

        int finalTaken = 0;
        int defense = 0;
        switch (attackType)
        {
            case AttackType.Physical:
                defense = hasTrueStrike ? 0 : GetSecondaryStat(SecondaryStat.PHDEF);
                if (defense > 0) TextOutputter.Instance.OutputText($"{name}'s defense reduced damage by {defense}.");
                finalTaken = TakeDamage(damageToTake - defense);
                break;
            case AttackType.Special:
                defense = hasTrueStrike ? 0 : GetSecondaryStat(SecondaryStat.SPDEF);
                if (defense > 0) TextOutputter.Instance.OutputText($"{name}'s special defense reduced damage by {defense}.");
                finalTaken = TakeDamage(damageToTake - defense);
                break;
            default:
                finalTaken = TakeDamage(damageToTake);
                break;
        }

        if (name == "Basilisk" && attacker != null && attacker.IsAlive() && finalTaken > 0)
        {
            int returnDmg = Mathf.RoundToInt(finalTaken * 0.30f);
            TextOutputter.Instance.OutputText($"{name} reflects {returnDmg} damage back to {attacker.GetName()}!");
            attacker.GetAttacked(returnDmg, AttackType.Physical, this);
        }

        if (attacker != null && finalTaken > 0)
        {
            attacker.OnDealDamage(finalTaken, this);
        }

        return finalTaken;
    }

    public override void OnDealDamage(int damage, Combatant target)
    {
        HandleLifeSteal(damage);
    }
}