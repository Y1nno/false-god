using UnityEngine;

[System.Serializable]
public abstract class ReligionReward
{
    public abstract void ApplyReward();
}

public enum ReligionRewardType
{
    DodgeChance,
}