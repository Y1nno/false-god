using UnityEngine;

[CreateAssetMenu(fileName = "StartOfCombatStatBuff", menuName = "Scriptable Objects/Religion/Rewards/Start Of Combat Stat Buff")]
public class StartOfCombatStatBuffReligionRewardSO : ReligionRewardSO
{
    public Stat stat;
    public float value;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.AddStartOfCombatStatBuff(stat, value);
    }

    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager?.RemoveStartOfCombatStatBuff(stat, value);
    }
}
