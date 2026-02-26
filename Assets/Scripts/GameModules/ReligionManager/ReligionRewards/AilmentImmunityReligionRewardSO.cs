using UnityEngine;

[CreateAssetMenu(fileName = "AilmentImmunity", menuName = "Scriptable Objects/Religion/Rewards/Ailment Immunity")]
public class AilmentImmunityReligionRewardSO : ReligionRewardSO
{
    public AilmentType ailmentType;

    public override void ApplyReward() { } //TODO: Pending ailment implementation
    public override void RemoveReward() { } //TODO: pending ailment implementation
}
