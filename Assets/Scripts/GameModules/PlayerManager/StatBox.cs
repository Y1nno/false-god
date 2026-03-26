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
        // { Stat.LCK, 10 }
    };

    public int STR = 10;
    public int DEX = 10;
    public int SPD = 10;
    public int INT = 10;
    // public int LCK = k_DefaultStatValue[Stat.LCK];

    public float ConsumableEffectivenessMultiplier { get; set; } = 1.0f;

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
            // Stat.LCK => LCK,
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
            /*
            case Stat.LCK:
                LCK = value;
                break;
            */
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }

    public virtual void OnNotify(object subject, EventType eventType) { }
}

public enum Stat
{
    STR,
    DEX,
    SPD,
    INT,
    // LCK
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