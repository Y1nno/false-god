using UnityEngine;
using System.Collections.Generic;

public class TrapEncounter : Encounter, IObserver
{
    private TrapEncounterSO _trapSO;
    private int _floor;
    private int _threshold;
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

        TextOutputter.Instance.OutputText($"You encountered a {_trapSO.TrapName}!");
        TextOutputter.Instance.OutputText(_trapSO.Description);

        List<string> choiceStrings = new List<string>
        {
            "Overcome (Strength)",
            "Dodge (Dexterity)",
            "Dismantle (Intelligence)"
        };

        new Prompt("How will you handle the trap?", choiceStrings, this);
    }

    private Stat _lastStatToRoll;
    private int _lastModifier;
    private bool _hasRerolled = false;
    private bool _waitingForRerollDecision = false;

    public override void RecieveDecision(int decisionIndex)
    {
        if (_waitingForRerollDecision)
        {
            _waitingForRerollDecision = false;
            if (decisionIndex == 0) // Yes
            {
                TextOutputter.Instance.OutputText("Serpent's Coil: You invoke the reroll!");
                _waitingForRoll = true;
                DiceRoller.Instance.RollForStatWithModifier(_lastStatToRoll, _lastModifier, _threshold);
            }
            else // No
            {
                TextOutputter.Instance.OutputText("You decline the reroll.");
                HandleRollResult();
            }
            return;
        }

        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        Stat statToRoll;
        int baseThreshold;

        switch (decisionIndex)
        {
            case 0:
                statToRoll = Stat.STR;
                baseThreshold = _trapSO.BaseOvercomeReq;
                break;
            case 1:
                statToRoll = Stat.DEX;
                baseThreshold = _trapSO.BaseDodgeReq;
                break;
            case 2:
                statToRoll = Stat.INT;
                baseThreshold = _trapSO.BaseDismantleReq;
                break;
            default:
                Debug.LogError("TrapEncounter: Invalid decision index.");
                ResolveEncounter();
                return;
        }

        _threshold = baseThreshold + ((_floor - 1) * 3);
        int statValue = pm.GetStat(statToRoll);
        int modifier = statValue / 2;

        _lastStatToRoll = statToRoll;
        _lastModifier = modifier;

        _waitingForRoll = true;
        DiceRoller.Instance.RollForStatWithModifier(statToRoll, modifier, _threshold);
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
        XPManager xm = RunManager.Instance.GetService<XPManager>();

        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (!result.IsSuccess && !_hasRerolled && rm?.CurrentReligion is SerpentsCoil && rm.CurrentReligion.CurrentFaithLevel >= 4)
        {
            _hasRerolled = true;
            _waitingForRerollDecision = true;
            new Prompt("You failed the trap check! Use Serpent's Coil reroll?", new List<string> { "Yes", "No" }, this);
            return;
        }

        int scaledDamage = _trapSO.BaseDamage + ((_floor - 1) * 10);
        int scaledExp = _trapSO.BaseExp + ((_floor - 1) * 3);

        if (result.IsSuccess)
        {
            float damageMultiplier = 0f;
            if (result.PlayerStat == Stat.STR)
            {
                damageMultiplier = 0.2f;
                TextOutputter.Instance.OutputText($"You braced yourself and powered through! You take reduced damage.");
            }
            else
            {
                TextOutputter.Instance.OutputText($"You successfully avoided the trap!");
            }

            int finalDamage = Mathf.RoundToInt(scaledDamage * damageMultiplier);
            if (finalDamage > 0) pm.TakeDamage(finalDamage);
            xm.AddXP(scaledExp);
        }
        else
        {
            TextOutputter.Instance.OutputText($"You failed to avoid the trap! You take full damage.");
            pm.TakeDamage(scaledDamage);
            xm.AddXP(Mathf.RoundToInt(scaledExp * 0.5f));
        }

        ResolveEncounter();
    }

    public override void ResolveEncounter()
    {
        DiceRoller.Instance.DetachObserver(this);
        base.ResolveEncounter();
    }
}
