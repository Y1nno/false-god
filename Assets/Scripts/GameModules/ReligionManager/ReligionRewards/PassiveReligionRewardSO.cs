using UnityEngine;

[CreateAssetMenu(fileName = "PassiveReward", menuName = "Scriptable Objects/Religion/Rewards/Passive")]
public class PassiveReligionRewardSO : ReligionRewardSO
{
    public Passive passive;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.AddPassive(passive);
    }

    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.RemovePassive(passive);
    }
}
