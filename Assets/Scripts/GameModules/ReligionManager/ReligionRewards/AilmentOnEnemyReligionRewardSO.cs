using UnityEngine;

[CreateAssetMenu(fileName = "AilmentOnEnemyReward", menuName = "Scriptable Objects/Religion/Rewards/Ailment On Enemy")]
public class AilmentOnEnemyReligionRewardSO : ReligionRewardSO
{
    public AilmentType ailmentType;
    public float chance;
    public int maxEnemies;
    public int encounterCooldown;

    public override void ApplyReward() { } //TODO: Pending ailment implementation
    public override void RemoveReward() { } //TODO: pending ailment implementation
}
