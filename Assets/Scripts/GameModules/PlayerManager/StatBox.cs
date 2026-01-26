using UnityEngine;
using System;
using System.Collections.Generic;

public class StatBox : Subject, IObserver
{
    private static readonly Dictionary<Stat, int> k_DefaultStatValue = new Dictionary<Stat, int>
    {
        { Stat.STR, 10 },
        { Stat.DEX, 10 },
        { Stat.INT, 10 },
        { Stat.LCK, 10 }
    };

    public int STR = k_DefaultStatValue[Stat.STR];
    public int DEX = k_DefaultStatValue[Stat.DEX];
    public int INT = k_DefaultStatValue[Stat.INT];
    public int LCK = k_DefaultStatValue[Stat.LCK];

    private readonly int k_StatPointsPerLevelUp = 5;
    public int AvailableStatPoints { get; private set; } = 0;

    public int GetStat(Stat stat)
    {
        return stat switch
        {
            Stat.STR => STR,
            Stat.DEX => DEX,
            Stat.INT => INT,
            Stat.LCK => LCK,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }

    public void SetStat(Stat stat, int value)
    {
        switch (stat)
        {
            case Stat.STR:
                STR = value;
                break;
            case Stat.DEX:
                DEX = value;
                break;
            case Stat.INT:
                INT = value;
                break;
            case Stat.LCK:
                LCK = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
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

public enum Stat
{
    STR,
    DEX,
    INT,
    LCK
}

public enum SecondaryStat
{
    SPATK,
    SPDEF,
    CRIT,
    EVDE
}