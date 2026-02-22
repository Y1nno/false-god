using UnityEngine;
using System;
using System.Collections.Generic;

public class StatBox : Subject, IObserver
{
    protected static readonly Dictionary<Stat, int> k_DefaultStatValue = new Dictionary<Stat, int>
    {
        { Stat.STR, 10 },
        { Stat.DEX, 10 },
        { Stat.SPD, 10 },
        { Stat.INT, 10 },
        { Stat.LCK, 10 }
    };

    public int STR = k_DefaultStatValue[Stat.STR];
    public int DEX = k_DefaultStatValue[Stat.DEX];
    public int SPD = k_DefaultStatValue[Stat.SPD];
    public int INT = k_DefaultStatValue[Stat.INT];
    public int LCK = k_DefaultStatValue[Stat.LCK];

    protected StatBox(Dictionary<Stat, int> initialStats = null)
    {
        if (initialStats == null)
        {
            return;
        }
        foreach (var stat in initialStats)
        {
            SetStat(stat.Key, stat.Value);
        }
    }

    public int GetStat(Stat stat)
    {
        return stat switch
        {
            Stat.STR => STR,
            Stat.DEX => DEX,
            Stat.SPD => SPD,
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
            case Stat.SPD:
                SPD = value;
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
}

public enum Stat
{
    STR,
    DEX,
    SPD,
    INT,
    LCK
}

public enum SecondaryStat
{
    SPATK,
    SPDEF,
    PHATK,
    PHDEF,
    CRIT,
    EVDE
}