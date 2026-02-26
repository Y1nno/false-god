using UnityEngine;
using System;

[Serializable]
public abstract class ReligionRewardSO : ScriptableObject
{
    public abstract void ApplyReward();
    public abstract void RemoveReward();
}
