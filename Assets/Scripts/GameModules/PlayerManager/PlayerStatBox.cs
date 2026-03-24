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

    public override void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.LevelUp:
                AddStatPoints(k_StatPointsPerLevelUp);
                break;
        }
    }
    public void AddStatPoints(int points)
    {
        AvailableStatPoints += points;
        TextOutputter.Instance.OutputText("You gained " + points + " stat points!");
        Notify(EventType.StatPointsAdded);
    }

    public bool SpendStatPoints(Stat stat, int points)
    {
        if (stat == Stat.SPD)
        {
            TextOutputter.Instance.OutputText("Speed cannot be increased manually. It scales from Dexterity and Items.");
            return false;
        }

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
