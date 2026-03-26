using UnityEngine;

public class SerpentsCoil : Religion
{
    public SerpentsCoil()
    {
        ReligionID = "Serpent's Coil";
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
                QuestDescription = "You have reached the peak of the Serpent.";
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
                pm?.IncreaseStat(Stat.SPD, 4);
                pm.CanUseHealthAsMana = true; // Ouroboros Rite effect
                TextOutputter.Instance.OutputText("Reward: +4 Permanent Speed and the Ouroboros Rite (Use Health as Mana).");
                break;
            case 2:
                dm?.GrantRandomReward(Rarity.Rare, EquipmentSlot.Legs);
                TextOutputter.Instance.OutputText("Reward: A pair of Blue tier Boots.");
                break;
            case 3:
                TextOutputter.Instance.OutputText("Reward: 35% chance to Poison random enemy at the start of battle.");
                break;
            case 4:
                pm.DiceRerollCount++;
                TextOutputter.Instance.OutputText("Reward: +1 Reroll on Traps and Shed Skin (20% chance to clear ailments).");
                break;
        }
    }
}
