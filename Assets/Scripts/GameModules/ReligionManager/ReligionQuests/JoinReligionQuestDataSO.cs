using UnityEngine;

[CreateAssetMenu(fileName = "New Join Religion Quest", menuName = "Scriptable Objects/Religion/Join Religion Quest")]
public class JoinReligionQuestDataSO : QuestDataSO
{

    public override bool CanComplete()
    {
        return true; //Always returns true for joining a religion
    }

    public override bool TryComplete()
    {
        return true; //Always returns true for joining a religion
    }

    public override string GetCurrentRequirementDescription()
    {
        return "Join {" + RunManager.Instance.GetService<ReligionManager>().CurrentReligion.ReligionName + "} religion to complete this quest.";
    }
}
