using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStatBox : StatBox
{
    private readonly int k_StatPointsPerLevelUp = 5;
    public int AvailableStatPoints { get; private set; } = 0;

    public PlayerStatBox(Dictionary<Stat, int> initialStats = null) : base(initialStats)
    {
        
    }

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.LevelUp:
                AddStatPoints(k_StatPointsPerLevelUp);
                break;
        }
    }
    private void AddStatPoints(int points)
    {
        AvailableStatPoints += points;
        Notify(EventType.StatPointsAdded);
    }

    public bool SpendStatPoints(Stat stat, int points)
    {
        if (points <= AvailableStatPoints)
        {
            int currentStatValue = GetStat(stat);
            SetStat(stat, currentStatValue + points);
            AvailableStatPoints -= points;
            return true;
        }
        return false;
    }
}
