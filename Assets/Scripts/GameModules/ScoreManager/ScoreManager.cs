using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : GameModule, IObserver
{
    public int CurrentScore { get; private set; }

    private Dictionary<EventType, int> k_ScorePerEvent = new Dictionary<EventType, int>
    {
        { EventType.EncounterResolve, 10 },
        { EventType.QuestComplete, 50 },
    };

    public override void AttachDefaultObservers()
    {
        RunManager.Instance.GetService<EncounterManager>()?.AttachObserver(this);
    }

    public void AddScore(int points)
    {
        CurrentScore += points;
        TotalScore += points;
        TextOutputter.Instance.OutputText($"Gained {points} points! Current Score: {CurrentScore} (Total: {TotalScore})");
        RunManager.Instance.GetService<SaveManager>()?.SaveRun();
    }

    public void RestoreState(int score)
    {
        CurrentScore = score;
    }

    public int TotalScore { get; private set; }
    public void RestoreMetaState(int totalScore)
    {
        TotalScore = totalScore;
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (k_ScorePerEvent.ContainsKey(eventType))
        {
            AddScore(k_ScorePerEvent[eventType]);
        }
    }
}