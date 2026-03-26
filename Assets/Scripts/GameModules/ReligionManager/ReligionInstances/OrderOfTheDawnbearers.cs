using UnityEngine;

public class OrderOfTheDawnbearers : Religion
{
    public OrderOfTheDawnbearers()
    {
        ReligionID = "Order of the Dawnbearers";
    }

    public override void AssignNextQuest()
    {
        switch (CurrentFaithLevel)
        {
            case 1:
                RequiredItemID = "144"; // Demon Heart
                QuestDescription = "Sacrifice a Demon Heart to the Altar.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 2:
                RequiredItemID = "159"; // Heretic Head
                QuestDescription = "Give the Dawnbearer apostle a Heretic head.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 3:
                RequiredEnemyName = "Boss";
                QuestDescription = "Defeat a Boss and reach the next religion encounter.";
                CurrentQuestStatus = QuestStatus.SlayingBoss;
                break;
            default:
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "You have reached the peak of the Dawn.";
                break;
        }
    }

    public override void GrantLevelReward(int level)
    {
        DropManager dm = RunManager.Instance.GetService<DropManager>();
        switch (level)
        {
            case 1:
                TextOutputter.Instance.OutputText("Reward: Healing Amplification +10%.");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Weapon);
                TextOutputter.Instance.OutputText("Reward: A heavy Blue tier Weapon.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: +15% bonus damage on your first physical attack of the battle.");
                break;
            case 4:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Chest);
                TextOutputter.Instance.OutputText("Reward: A shining Blue tier Armor.");
                break;
        }
    }
}
