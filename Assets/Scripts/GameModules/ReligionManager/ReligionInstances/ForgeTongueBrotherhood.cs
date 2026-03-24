using UnityEngine;

public class ForgeTongueBrotherhood : Religion
{
    public ForgeTongueBrotherhood()
    {
        ReligionID = "Forge-Tongue Brotherhood";
    }

    public override void AssignNextQuest()
    {
        switch (CurrentFaithLevel)
        {
            case 1:
                RequiredItemID = "145"; // Living Stone
                QuestDescription = "Give a Living Stone to the Blacksmith.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 2:
                RequiredItemID = "140"; // Ogre Ear
                QuestDescription = "Give an Ogre's Ear to the Blacksmith.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 3:
                RequiredEnemyName = "Boss";
                QuestDescription = "Defeat a Boss and reach the next religion encounter.";
                CurrentQuestStatus = QuestStatus.SlayingBoss;
                break;
            default:
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "The forge burns bright. You have reached the peak.";
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
                TextOutputter.Instance.OutputText("Reward: Free Repairs at the Blacksmith.");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Chest);
                TextOutputter.Instance.OutputText("Reward: A sturdy Blue tier Armor.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: Blacksmith upgrades equipment to +5 for 0 gold (and no Gemshard).");
                break;
            case 4:
                TextOutputter.Instance.OutputText("Reward: Forge Blessing (Weapons don't lose durability) & Improved Blacksmith Stock.");
                break;
        }
    }
}
