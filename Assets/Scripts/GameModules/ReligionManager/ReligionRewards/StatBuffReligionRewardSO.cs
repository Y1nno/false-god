using UnityEngine;

[CreateAssetMenu(fileName = "StatBuffReward", menuName = "Scriptable Objects/Religion/Rewards/Stat Buff")]
public class StatBuffReligionRewardSO : ReligionRewardSO
{
    public Stat stat;
    public int value;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.SetStat(stat, playerManager.GetStat(stat) + value);
    }
    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.SetStat(stat, playerManager.GetStat(stat) - value);
    }
}
