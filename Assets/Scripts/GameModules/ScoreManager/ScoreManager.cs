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
        // No default observers for now
    }
    
    public void AddScore(int points)
    {
        CurrentScore += points;
        TextOutputter.Instance.OutputText($"Gained {points} points! Current Score: {CurrentScore}");
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (k_ScorePerEvent.ContainsKey(eventType))
        {
            AddScore(k_ScorePerEvent[eventType]);
        }
    }
}