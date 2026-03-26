using UnityEngine;

public class ChildrenOfThePaleMoon : Religion
{
    public ChildrenOfThePaleMoon()
    {
        ReligionID = "Children of the Pale Moon";
    }

    public override void AssignNextQuest()
    {
        switch (CurrentFaithLevel)
        {
            case 1:
                RequiredItemID = "143"; // Kobold Horn
                QuestDescription = "Sacrifice a Kobold's Horn to the Altar.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 2:
                RequiredItemID = "144"; // Demon Heart
                QuestDescription = "Sacrifice a Demon Heart to the Altar.";
                CurrentQuestStatus = QuestStatus.GatheringItem;
                break;
            case 3:
                RequiredEnemyName = "Boss";
                QuestDescription = "Defeat a Boss and reach the next religion encounter.";
                CurrentQuestStatus = QuestStatus.SlayingBoss;
                break;
            default:
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "You have reached the peak of the Pale Moon.";
                break;
        }
    }

    public override void GrantLevelReward(int level)
    {
        DropManager dm = RunManager.Instance.GetService<DropManager>();
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        DungeonManager dungeonm = RunManager.Instance.GetService<DungeonManager>();

        switch (level)
        {
            case 1:
                TextOutputter.Instance.OutputText("Reward: 10% Mana recovery after every encounter.");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.OffHand);
                TextOutputter.Instance.OutputText("Reward: A mysterious Blue tier Catalyst.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: +20% Special Attack when HP is above 80%.");
                break;
            case 4:
                if (pm != null && dungeonm != null)
                {
                    int bonus = dungeonm.CurrentDungeonFloor * 5;
                    pm.Mana.IncreaseBaseMax(bonus);
                    pm.RestoreMana(bonus);
                    TextOutputter.Instance.OutputText($"Reward: Retroactive +5 Max Mana for every depth reached (Total: +{bonus}).");
                    
                    RunManager.Instance.GetService<RiteManager>()?.UnlockRite(RiteType.Empress.ToString());
                }
                break;
        }
    }
}
