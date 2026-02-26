using UnityEngine;

[CreateAssetMenu(fileName = "ManaRecoveryReward", menuName = "Scriptable Objects/Religion/Rewards/Mana Recovery")]
public class ManaRecoveryReligionRewardSO : ReligionRewardSO
{
    [Range(0f, 1f)]
    public float manaRecoveryPercent;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.AddManaRecoveryPercent(manaRecoveryPercent);
    }
    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.RemoveManaRecoveryPercent(manaRecoveryPercent);
    }
}
