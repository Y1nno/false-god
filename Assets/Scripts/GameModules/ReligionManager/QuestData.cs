using System;
using UnityEngine;

public enum QuestType
{
    JoinReligion,
    GiveItem,
    KillBoss
}

[Serializable]
public class QuestData
{
    public QuestType type;

    // Common fields (optional)
    public int goal;            // how many / how much needed
    public int rewardGold;      // example common reward field

    // GiveItem params
    public int itemID;
    public int amount;

    // KillBoss params
    public int bossID;
}