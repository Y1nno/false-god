using UnityEngine;
using System.Collections.Generic;

public class TrapEncounter : Encounter, IObserver
{
    private TrapEncounterSO _trapSO;
    private int _floor;
    private int _threshold;
    private TrapChoice _selectedChoice;
    private bool _waitingForRoll = false;

    public TrapEncounter(float difficulty, TrapEncounterSO trapSO, int floor) : base(difficulty)
    {
        _trapSO = trapSO;
        _floor = floor;
        DiceRoller.Instance.AttachObserver(this);
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        if (_trapSO == null)
        {
            Debug.LogError("TrapEncounter: TrapEncounterSO is missing! Resolving encounter immediately.");
            ResolveEncounter();
            return;
        }

        TextOutputter.Instance.OutputText(_trapSO.Description);
        List<string> choiceStrings = new List<string>();
        foreach (var choice in _trapSO.Choices)
        {
            choiceStrings.Add($"Attempt to dodge using {choice.StatToRoll}");
        }

        Prompt trapPrompt = new Prompt("How will you avoid the trap?", choiceStrings, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (decisionIndex < 0 || decisionIndex >= _trapSO.Choices.Count)
        {
            Debug.LogError("TrapEncounter: Invalid decision index.");
            ResolveEncounter();
            return;
        }

        _selectedChoice = _trapSO.Choices[decisionIndex];
        _threshold = _selectedChoice.RelativeDifficulty + _floor;
        
        _waitingForRoll = true;
        DiceRoller.Instance.RollForStat(_selectedChoice.StatToRoll, _threshold);
    }

    public void OnNotify(object subject, EventType eventType)
    {
        
        if (_waitingForRoll && eventType == EventType.DiceRollFinalized)
        {
            _waitingForRoll = false;
            HandleRollResult();
        }
    }

    private void HandleRollResult()
    {
        DiceRollResult result = DiceRoller.Instance.LastRollResult;
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();

        if (result.IsSuccess)
        {
            TextOutputter.Instance.OutputText($"You successfully avoided the trap! You take {_selectedChoice.PassDamage} damage.");
            pm.TakeDamage(_selectedChoice.PassDamage);
        }
        else
        {
            TextOutputter.Instance.OutputText($"You failed to avoid the trap! You take {_selectedChoice.FailDamage} damage.");
            pm.TakeDamage(_selectedChoice.FailDamage);
        }

        ResolveEncounter();
    }

    public override void ResolveEncounter()
    {
        DiceRoller.Instance.DetachObserver(this);
        base.ResolveEncounter();
    }
}
