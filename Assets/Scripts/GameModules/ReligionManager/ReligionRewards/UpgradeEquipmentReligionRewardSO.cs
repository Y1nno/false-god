using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeEquipmentReward", menuName = "Scriptable Objects/Religion/Rewards/Upgrade Equipment")]
public class UpgradeEquipmentReligionRewardSO : ReligionRewardSO
{
    public int upgradeAmount;

    public override void ApplyReward() { } // TODO: Pending equipment upgrade implementation
    public override void RemoveReward() { }
}
