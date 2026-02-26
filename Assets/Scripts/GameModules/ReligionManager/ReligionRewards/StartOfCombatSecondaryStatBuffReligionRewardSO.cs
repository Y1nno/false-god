using UnityEngine;

[CreateAssetMenu(fileName = "StartOfCombatSecondaryStatBuff", menuName = "Scriptable Objects/Religion/Rewards/Start Of Combat Secondary Stat Buff")]
public class StartOfCombatSecondaryStatBuffReligionRewardSO : ReligionRewardSO
{
    public SecondaryStat stat;
    public float value;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.AddStartOfCombatSecondaryStatBuff(stat, value);
    }

    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.RemoveStartOfCombatSecondaryStatBuff(stat, value);
    }
}
