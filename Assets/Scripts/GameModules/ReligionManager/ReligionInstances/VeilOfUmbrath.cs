using UnityEngine;

public class VeilOfUmbrath : Religion
{
    public VeilOfUmbrath()
    {
        ReligionID = "The Veil of Umbrath";
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
                RequiredItemID = "141"; // Cloaker Skin
                QuestDescription = "Give the Umbrath apostle a Cloaker skin.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 3:
                RequiredEnemyName = "Boss";
                QuestDescription = "Defeat a Boss and reach the next religion encounter.";
                CurrentQuestStatus = QuestStatus.SlayingBoss;
                break;
            default:
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "You have reached the peak of Umbrath.";
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
                TextOutputter.Instance.OutputText("Reward: +30% Dodge on the first round of battle.");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Accessory1);
                TextOutputter.Instance.OutputText("Reward: A mysterious Blue tier Ring.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: +40% Crit Chance on the first round of battle.");
                break;
            case 4:
                pm?.PlayerStats.AddStatPoints(15);
                TextOutputter.Instance.OutputText("Reward: +15 Attribute points to allocate.");
                break;
        }
    }
}
