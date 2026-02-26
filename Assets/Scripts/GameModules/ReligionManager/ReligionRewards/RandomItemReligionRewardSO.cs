using UnityEngine;

[CreateAssetMenu(fileName = "RandomItemReward", menuName = "Scriptable Objects/Religion/Rewards/Random Item")]
public class RandomItemReligionRewardSO : ReligionRewardSO
{
    public EquipmentSlot slot;
    public Rarity rarity;

    public override void ApplyReward()
    {
        //TODO: Implement item generation and addition to player's inventory once added to develop
    }

    public override void RemoveReward()
    {
        // No implementation necessary
    }
}