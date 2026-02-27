using UnityEngine;

[CreateAssetMenu(fileName = "New Kill Boss Quest", menuName = "Scriptable Objects/Religion/Kill Boss Quest")]
public class KillBossQuestDataSO : QuestDataSO
{
    public int bossID;

    public override bool CanComplete()
    {
        return RunManager.Instance.GetService<CombatManager>().HasDefeatedBoss(bossID);
    }

    public override bool TryComplete()
    {
        return CanComplete();
    }

    public override string GetCurrentRequirementDescription()
    {
        return "Defeat the boss with ID " + bossID + " to complete this quest.";
    }
}