using UnityEngine;
using System;

public enum ConsumableEffectType
{
    FlatHP,
    PercentHP,
    FlatMana,
    PercentMana,
    StatChange,
    AllStatsChange,
    HealOverTime,
    ManaOverTime,
    Portal,
    GoldGain
}

public enum EffectDurationType
{
    Instant,
    Turns,
    Encounter
}

[Serializable]
public struct ConsumableEffect
{
    public ConsumableEffectType Type;
    public EffectDurationType DurationType;

    [Tooltip("Amounts for each tier. Index 0 = Tier 1, Index 1 = Tier 2, etc. If only 1 amount, it's used for all tiers.")]
    public float[] Amount;

    [Tooltip("How many rounds the effect lasts. Use 1 or 0 for instant effects. Ignored if DurationType is Encounter.")]
    public int Duration;

    [Tooltip("Which stat to modify, if Type is StatChange")]
    public Stat TargetStat;
}
