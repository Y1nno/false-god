using UnityEngine;

[CreateAssetMenu(fileName = "New Give Item Quest", menuName = "Scriptable Objects/Religion/Give Item Quest")]
public class GiveItemQuestDataSO : QuestDataSO
{
    public int requiredItemID;
    public int requiredItemAmount;
}
