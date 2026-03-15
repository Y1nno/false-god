using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : Combatant, IPromptResponder
{
    private PlayerManager _pm => RunManager.Instance.GetService<PlayerManager>();

    private List<CombatAction> _availableActions = new List<CombatAction>();
    private enum DecisionMode { None, ChoosingAction, ChoosingTarget }
    private DecisionMode _currentDecisionMode = DecisionMode.None;

    private readonly List<int> k_StartingActionIDs = new List<int>
    {
        0, // Do Nothing
        1, // Basic Attack
        2, // Basic Block
        3, // Fireball
        999, // Switch Weapon
    };

    private List<int> _decisionsAvailable = new List<int>();

    List<Combatant> possibleTargets = new List<Combatant>();

    public PlayerCombatManager()
    {
        foreach (int actionID in k_StartingActionIDs)
        {
            CombatAction action = ActionFactory.CreateActionByID(actionID);
            if (action != null)
            {
                _availableActions.Add(action);
            }
        }
    }
    public override Resource GetHealth()
    {
        return _pm.Health;
    }
    public override Resource GetMana()
    {
        return _pm.Mana;
    }

    protected override int TakeDamage(int amount)
    {
        // Redirect damage to PlayerManager to ensure global events (like Lazarus Rite) trigger
        return _pm.TakeDamage(amount);
    }

    public override int GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null)
    {
        // Spell Reflection Interceptor
        if (attackType == AttackType.Special && attacker != null)
        {
            EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
            if (eqm != null)
            {
                int reflectChance = eqm.GetTotalSpellReflectChance();
                if (reflectChance > 0 && UnityEngine.Random.Range(0, 100) < reflectChance)
                {
                    TextOutputter.Instance.OutputText($"{GetName()} reflected the spell back at {attacker.GetName()}!");
                    return attacker.GetAttacked(damage, AttackType.Special, this);
                }
            }
        }

        if (damage > 0)
        {
            EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
            if (eqm != null)
            {
                if (attackType == AttackType.Physical)
                {
                    eqm.DegradeEquippedArmor(); // Drain armor durability on physical hits taken
                }

                int blockChance = eqm.GetTotalBlockChance();
                if (blockChance > 0 && UnityEngine.Random.Range(0, 100) < blockChance)
                {
                    int blockAmt = eqm.GetTotalBlockAmount();
                    damage -= blockAmt;
                    TextOutputter.Instance.OutputText($"{GetName()} blocked the attack! Mitigated {blockAmt} damage.");
                }
            }
        }

        if (damage <= 0) 
        {
            TextOutputter.Instance.OutputText($"{GetName()} blocked all incoming damage!");
            return 0;
        }

        if (damage > 0 && attacker != null && attacker is Enemy)
        {
            RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
            if (relicm != null)
            {
                damage = Mathf.RoundToInt(damage * relicm.GetEnemyDamageMultiplier());
            }
        }

        int damageDealt = 0;
        switch (attackType)
        {
            case AttackType.Physical:
                damageDealt = TakeDamage(damage - _pm.CalculateSecondaryStat(SecondaryStat.PHDEF));
                break;
            case AttackType.Special:
                damageDealt = TakeDamage(damage - _pm.CalculateSecondaryStat(SecondaryStat.SPDEF));
                break;
            default:
                damageDealt = TakeDamage(damage);
                break;
        }

        if (IsAlive() && attackType == AttackType.Physical && attacker != null && attacker.IsAlive())
        {
            EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
            if (eqm != null && eqm.HasTraitAvailable(EquipmentTrait.CounterChance, out EquipmentSO item, out int traitIndex))
            {
                float counterChance = item.Traits[traitIndex].Value;
                if (UnityEngine.Random.Range(0f, 100f) < counterChance)
                {
                    TextOutputter.Instance.OutputText($"{GetName()} counters the attack!");
                    PlayerAttackAction counterAttack = new PlayerAttackAction();
                    counterAttack.Execute(this, attacker); // Counter-attacks can inherit DoubleStrike and ComboStrike intrinsically!
                }
            }
        }

        return damageDealt;
    }

    public override float GetCritChance()
    {
        return _pm.CritChance;
    }

    public override void Die()
    {
        TextOutputter.Instance.OutputText("You have been defeated!");
    }
    public override void ChooseAction()
    {
        if (HasAilment(AilmentType.Frozen))
        {
            TextOutputter.Instance.OutputText("You are frozen solid and cannot move!");
            _availableActions.Clear();
            _availableActions.Add(ActionFactory.CreateActionByID(000)); // Do Nothing action
            
            Notify(EventType.PlayerTurnStart);
            _currentDecisionMode = DecisionMode.ChoosingAction;
            Prompt frozenPrompt = new Prompt("You are frozen!", new List<string> { "Skip Turn" }, this);
            return;
        }
        _availableActions.Clear();
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        EquipmentSO moveFirstItem = null;
        int traitIndex = -1;
        bool hasMoveFirst = eqm != null && eqm.HasMoveFirstAvailable(out moveFirstItem, out traitIndex);

        foreach (int actionID in k_StartingActionIDs)
        {
            CombatAction action = ActionFactory.CreateActionByID(actionID);
            if (action != null)
            {
                _availableActions.Add(action);
            }
        }

        Notify(EventType.PlayerTurnStart);
        _currentDecisionMode = DecisionMode.ChoosingAction;
        List<string> actionNames = GetAvailableActionNames();
        Prompt prompt = new Prompt("Choose your action:", actionNames, this);
    }
    public override void ExecuteAction()
    {
        if (CurrentAction != null)
        {
            if (CurrentAction.Priority == 1 && CurrentAction.ActionID == 1) 
            {
               EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
               eqm?.TriggerMoveFirstCooldown();
            }
            CurrentAction.Execute(this, _currentTarget);
        }
        CurrentAction = null;
        _currentTarget = null;

    }

    public void ProcessPromptResponse(int decisionIndex)
    {
        RecieveDecision(decisionIndex);
    }

    public void RecieveDecision(int decisionIndex)
    {
        //Debug.Log($"RecieveDecision called: idx={decisionIndex}, mode={_currentDecisionMode}, frame={Time.frameCount}");
        //Debug.Log(observersString);

        switch (_currentDecisionMode)
        {
            case DecisionMode.None:
                //Debug.LogWarning("Received decision while not in decision mode.");
                break;

            case DecisionMode.ChoosingAction:
                //Debug.Log($"Player selected action index: {decisionIndex}");
                CombatAction selectedAction = _availableActions[decisionIndex];

                if (selectedAction.ActionID == 1) // Basic Attack
                {
                    EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
                    if (eqm != null && eqm.HasMoveFirstAvailable(out EquipmentSO item, out int traitIndex))
                    {
                        if (item.Traits[traitIndex].CurrentCooldown > 0)
                        {
                            selectedAction.Priority = 0;
                        }
                        else
                        {
                            selectedAction.Priority = 1;
                        }
                    }
                    else
                    {
                        selectedAction.Priority = 0;
                    }
                }

                TextOutputter.Instance.OutputText($"Player selected action: {selectedAction.ActionName}");
                CurrentAction = selectedAction;
                if (CurrentAction.NeedsTarget() == false)
                {
                    _currentDecisionMode = DecisionMode.None;
                    //Debug.Log("Selected action does not require target, setting action and notifying.");
                    Notify(EventType.PlayerActionSet);
                    return;
                }

                _currentDecisionMode = DecisionMode.ChoosingTarget;
                possibleTargets = CurrentAction.GetAvailableTargets(this, CurrentAction.TargetType);
                List<string> targetStrings = new List<string>();
                foreach (Combatant target in possibleTargets)
                {
                    targetStrings.Add($"{target.GetName()} (HP: {target.GetHealth().CurrentValue}/{target.GetHealth().MaxValue})");
                }
                Prompt prompt = new Prompt("Choose your target:", targetStrings, this);
                break;

            case DecisionMode.ChoosingTarget:
                _currentDecisionMode = DecisionMode.None;
                _currentTarget = possibleTargets[decisionIndex];
                possibleTargets.Clear();
                //Debug.Log($"Selected target: {_currentTarget.GetName()}, setting action and notifying.");
                Notify(EventType.PlayerActionSet);
                break;
        }
    }

    public override string GetName()
    {
        return "Player";
    }
    public override bool IsAlive()
    {
        return _pm.Health.CurrentValue > 0;
    }
    public override int GetStat(Stat stat)
    {
        int baseStat = _pm.GetStat(stat);
        return baseStat + GetStatBonus(stat);
    }

    public int GetStatBonus(Stat stat)
    {
        float bonus = 0;
        if (_activeEffects == null) return 0;
        foreach (var effect in _activeEffects)
        {
            if ((effect.BaseEffect.Type == ConsumableEffectType.StatChange && effect.BaseEffect.TargetStat == stat) ||
                effect.BaseEffect.Type == ConsumableEffectType.AllStatsChange)
            {
                bonus += effect.ModifiedAmount;
            }
        }
        return Mathf.RoundToInt(bonus);
    }

    public override int GetSecondaryStat(SecondaryStat stat)
    {
        int baseStat = _pm.CalculateSecondaryStat(stat);

        if (HasAilment(AilmentType.Burn) && stat == SecondaryStat.PHATK)
        {
            baseStat = Mathf.RoundToInt(baseStat * 0.90f);
        }
        else if (HasAilment(AilmentType.Poison) && stat == SecondaryStat.SPDEF)
        {
            baseStat = Mathf.RoundToInt(baseStat * 0.95f);
        }

        if (HasAilment(AilmentType.Frozen))
        {
            if (stat == SecondaryStat.PHDEF || stat == SecondaryStat.SPDEF)
            {
                baseStat = Mathf.RoundToInt(baseStat * 1.20f);
            }
        }

        return baseStat + GetSecondaryStatBonus(stat);
    }

    public int GetSecondaryStatBonus(SecondaryStat stat)
    {
        // Currently ConsumableEffectType doesn't distinguish secondary stats specifically in its TargetStat field (which is only 'Stat' enum),
        // but if you add more specific consumable types later, this is where you'd aggregate those bonuses.
        return 0;
    }
    public override int GetBonusDamage()
    {
        return _pm.BonusDamage;
    }

    public override bool TryUseMana(int amount)
    {
        // 1. Try normal mana usage first (includes Blessed Cross check)
        if (_pm.TryUseMana(amount))
        {
            return true;
        }

        // 2. If out of mana, check for Ouroboros Rite
        if (_pm.CanUseHealthAsMana)
        {
            int currentMana = _pm.Mana.CurrentValue;
            int deficit = amount - currentMana;

            // Consume all remaining mana
            if (currentMana > 0)
            {
                _pm.Mana.Decrease(currentMana);
            }

            // Consume health for the rest
            if (_pm.Health.CurrentValue > deficit)
            {
                _pm.TakeDamage(deficit);
                TextOutputter.Instance.OutputText($"Ouroboros: Consumed {deficit} HP for Mana!");
                return true;
            }
            else
            {
                 TextOutputter.Instance.OutputText("Not enough Health to cast spell!");
                 return false;
            }
        }

        return false;
    }

    private List<string> GetAvailableActionNames()
    {
        List<string> actionNames = new List<string>();
        foreach (CombatAction action in _availableActions)
        {
            actionNames.Add(action.ActionName);
        }
        return actionNames;
    }

    public override bool CanAffordMana(int amount)
    {
        if (base.CanAffordMana(amount)) return true;

        if (_pm.CanUseHealthAsMana)
        {
            int deficit = amount - _pm.Mana.CurrentValue;
            // Ensure we have enough health to cover the deficit
            return _pm.Health.CurrentValue > deficit;
        }
        return false;
    }
}
