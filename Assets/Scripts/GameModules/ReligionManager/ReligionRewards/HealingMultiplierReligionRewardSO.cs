using UnityEngine;

[CreateAssetMenu(fileName = "HealingMultiplierReward", menuName = "Scriptable Objects/Religion/Rewards/Healing Multiplier")]
public class HealingMultiplierReligionRewardSO : ReligionRewardSO
{
    [Tooltip("Float to ADD to healing, 0.1 = 10% more healing.")]
    public float multiplier;

    public override void ApplyReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.HealingMultiplier += multiplier;
    }

    public override void RemoveReward()
    {
        PlayerManager playerManager = RunManager.Instance.GetService<PlayerManager>();
        playerManager.HealingMultiplier = Mathf.Max(0f, playerManager.HealingMultiplier - multiplier);
    }
}
