using UnityEngine;

[CreateAssetMenu(fileName = "FreeRepairAtBlacksmithReward", menuName = "Scriptable Objects/Religion/Rewards/Free Repair At Blacksmith")]
public class FreeRepairAtBlacksmithReligionRewardSO : ReligionRewardSO
{
    public int freeRepairs;
    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.freeRepairs += freeRepairs;
    }

    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.freeRepairs -= freeRepairs;
    }
}
