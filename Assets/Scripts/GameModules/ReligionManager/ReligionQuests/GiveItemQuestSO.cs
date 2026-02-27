using UnityEngine;

[CreateAssetMenu(fileName = "New Give Item Quest", menuName = "Scriptable Objects/Religion/Give Item Quest")]
public class GiveItemQuestDataSO : QuestDataSO
{
    public int requiredItemID;
    public int requiredItemAmount;
    public override bool CanComplete()
    {
        return RunManager.Instance.GetService<InventoryManager>().GetItemCount(requiredItemID) >= requiredItemAmount;
    }

    public override bool TryComplete()
    {
        for (int i = 0; i < requiredItemAmount; i++)
        {
            RunManager.Instance.GetService<InventoryManager>().RemoveItemFromInventory(requiredItemID);
        }
        return true;
    }

    public override string GetCurrentRequirementDescription()
    {
        return "Give " + requiredItemAmount + " of item ID " + requiredItemID + " to complete this quest.";
    }
}
