using UnityEngine;

[CreateAssetMenu(fileName = "SpecificItemReward", menuName = "Scriptable Objects/Religion/Rewards/Specific Item")]
public class SpecificItemReligionRewardSO : ReligionRewardSO
{
    public int itemID;

    public override void ApplyReward()
    {
        //TODO: Implement item generation and addition to player's inventory once added to develop
    }

    public override void RemoveReward()
    {
        // No implementation necessary
    }
}
