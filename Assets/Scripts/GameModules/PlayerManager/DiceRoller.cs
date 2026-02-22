using System;
using System.Collections.Generic;
using UnityEngine.UI;
public struct DiceRollResult
{
    public Stat PlayerStat;
    public int BaseRoll;
    public int StatModifier;
    public int Total;
    public int SuccessThreshold;
    public bool IsSuccess;
    public int RollNumber;
}

public class DiceRoller : Subject, IObserver, IPromptResponder
{
    private static DiceRoller _instance;

    public DiceRollResult LastRollResult { get; private set; }
    public bool HasRollResult { get; private set; }

    private readonly int k_minRoll = 1;
    private readonly int k_maxRoll = 20;
    private PlayerManager _playerManager;

    public static DiceRoller Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DiceRoller();
                _instance._playerManager = RunManager.Instance.GetService<PlayerManager>();
            }
            return _instance;
        }
    }

    public void RollForStat(Stat statType, int successThreshold, int rollCounter = 0)
    {

        StatBox statBox = _playerManager.PlayerStats;

        int baseRoll = UnityEngine.Random.Range(k_minRoll, k_maxRoll + 1);
        int modifier = statBox.GetStat(statType);
        int total = baseRoll + modifier;

        var result = new DiceRollResult
        {
            PlayerStat = statType,
            BaseRoll = baseRoll,
            StatModifier = modifier,
            Total = total,
            SuccessThreshold = successThreshold,
            IsSuccess = total >= successThreshold,
            RollNumber = ++rollCounter
        };

        LastRollResult = result;
        HasRollResult = true;
        TextOutputter.Instance.OutputText($"You have rolled a {statType} check, result: {total}.");

        if (result.RollNumber <= _playerManager.DiceRerollCount)
        {
            TextOutputter.Instance.OutputText($"You have { _playerManager.DiceRerollCount - result.RollNumber + 1} rerolls left.");
            Prompt rerollPrompt = new Prompt("Would you like to reroll?", new List<string> { "Yes", "No" }, this);
        }
        Notify(EventType.DiceRoll);
        return;
    }

    public int RollD20()
    {
        return UnityEngine.Random.Range(1, 20 + 1);
    }

    public void Reroll()
    {
        if (!HasRollResult)
        {
            throw new InvalidOperationException("You cannot reroll before a first roll exists.");
        }
        if (LastRollResult.RollNumber > _playerManager.DiceRerollCount)
        {
            TextOutputter.Instance.OutputText("You have no rerolls left.");
            return;
        }

        RollForStat(LastRollResult.PlayerStat, LastRollResult.SuccessThreshold, LastRollResult.RollNumber);
    }

    public void ProcessPromptResponse(int decisionIndex)
    {
        switch (decisionIndex)
        {
            case 0:
                Reroll();
                break;
            case 1:
                TextOutputter.Instance.OutputText("You chose not to reroll.");
                break;
            default:
                TextOutputter.Instance.OutputText("Invalid decision index for dice roll.");
                break;
        }
    }
}

