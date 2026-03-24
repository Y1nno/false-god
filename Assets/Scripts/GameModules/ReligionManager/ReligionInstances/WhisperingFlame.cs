using UnityEngine;

public class WhisperingFlame : Religion
{
    public WhisperingFlame()
    {
        ReligionID = "Coven of the Whispering Flame";
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
                RequiredItemID = "142"; // Banshee Hair
                QuestDescription = "Give the apostle a lock of Banshee Hair.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 3:
                RequiredEnemyName = "Boss";
                QuestDescription = "Defeat a Boss and reach the next religion encounter.";
                CurrentQuestStatus = QuestStatus.SlayingBoss;
                break;
            default:
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "The flame whispers your name. You have reached the peak.";
                break;
        }
    }

    public override void GrantLevelReward(int level)
    {
        DropManager dm = RunManager.Instance.GetService<DropManager>();
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();

        switch (level)
        {
            case 1:
                TextOutputter.Instance.OutputText("Reward: Burn Immunity.");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Head);
                TextOutputter.Instance.OutputText("Reward: A seared Blue tier Helmet.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: 15% chance to Burn a random enemy at the start of battle.");
                break;
            case 4:
                TextOutputter.Instance.OutputText("Reward: Fervor (Kill enemy in 4 turns for stacking ATK buff).");
                break;
        }
    }
}
