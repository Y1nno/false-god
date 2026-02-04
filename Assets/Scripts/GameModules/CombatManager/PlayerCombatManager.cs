using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : Combatant
{
    private PlayerManager _pm = RunManager.Instance.GetService<PlayerManager>();

    private List<CombatAction> _availableActions = new List<CombatAction>();
    private enum DecisionMode { None, ChoosingAction, ChoosingTarget }
    private DecisionMode _currentDecisionMode = DecisionMode.None;

    private readonly List<int> k_StartingActionIDs = new List<int>
    {
        0, // Do Nothing
        1, // Basic Attack
        2, // Basic Block
        3, // Fireball
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
    public override void Die()
    {
        TextOutputter.Instance.OutputText("You have been defeated!");
    }
    public override void ChooseAction()
    {
        Notify(EventType.PlayerTurnStart);
        _currentDecisionMode = DecisionMode.ChoosingAction;
        List<string> actionNames = GetAvailableActionNames();
        OutputEnumeratedDecisionOptions(actionNames, "Choose your action:");
    }
    public override void ExecuteAction()
    {
        if (CurrentAction != null)
        {
            CurrentAction.Execute(this, _currentTarget);
        }
        CurrentAction = null;
        _currentTarget = null;

    }

    public void RecieveDecision(int decisionIndex)
    {
        Debug.Log($"RecieveDecision called: idx={decisionIndex}, mode={_currentDecisionMode}, frame={Time.frameCount}");

        if (!ValidateDecisionIndex(decisionIndex))
        {
            Debug.LogWarning("Invalid decision index received: " + decisionIndex);
            return;
        }

        switch (_currentDecisionMode)
        {
            case DecisionMode.None:
                Debug.LogWarning("Received decision while not in decision mode.");
                break;

            case DecisionMode.ChoosingAction:
                TextOutputter.Instance.OutputText($"Player selected action: {_availableActions[decisionIndex].ActionName}");
                CurrentAction = _availableActions[decisionIndex];
                if (CurrentAction.NeedsTarget() == false)
                {
                    _currentDecisionMode = DecisionMode.None;
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
                OutputEnumeratedDecisionOptions(targetStrings, "Choose your target:");
                break;

            case DecisionMode.ChoosingTarget:
                _currentDecisionMode = DecisionMode.None;
                _currentTarget = possibleTargets[decisionIndex];
                possibleTargets.Clear();
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
        return _pm.GetStat(stat);
    }

    public bool ValidateDecisionIndex(int decisionIndex)
    {
        //Debug.Log($"Validating decision index: {decisionIndex}");
        //Debug.Log($"Decisions available count: {_decisionsAvailable.Count}");
        //Debug.Log($"Is decision index valid: {decisionIndex >= 0 && decisionIndex < _decisionsAvailable.Count}");
        return decisionIndex >= 0 && decisionIndex < _decisionsAvailable.Count;
    }

    public void OutputEnumeratedDecisionOptions(List<string> options, string header = "Choose an option:")
    {
        _decisionsAvailable.Clear();
        string outputText = $"{header}\n";
        for (int i = 0; i < options.Count; i++)
        {
            outputText += $"{i + 1}. {options[i]}\n";
            _decisionsAvailable.Add(i);
        }
        TextOutputter.Instance.OutputText(outputText);
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

    public void SetCurrentTarget(Combatant target)
    {
        _currentTarget = target;
    }
}
