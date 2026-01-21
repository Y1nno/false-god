using UnityEngine;
using System;
using System.Collections.Generic;

public class StatBox
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