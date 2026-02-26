using UnityEngine;

[CreateAssetMenu(fileName = "StatPointsReward", menuName = "Scriptable Objects/Religion/Rewards/Stat Points")]
public class StatPointsReligionRewardSO : ReligionRewardSO
{
    public int points;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.PlayerStats.AddStatPoints(points);
    }

    public override void RemoveReward()
    {
        // No implementation necessary
    }
}
