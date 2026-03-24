using UnityEngine;

public enum QuestStatus { None, GatheringItem, SlayingBoss, ReadyToTurnIn }

public class Religion
{
    public string ReligionID { get; protected set; }
    public int CurrentFaithLevel { get; protected set; } = 1;
    public QuestStatus CurrentQuestStatus { get; protected set; } = QuestStatus.None;
    public string RequiredItemID { get; protected set; } = "";
    public string RequiredEnemyName { get; protected set; } = "";
    public string QuestDescription { get; protected set; } = "No active quest.";

    public virtual void OnJoinReligion()
    {
        Debug.Log("Joined religion: " + ReligionID);
        TextOutputter.Instance.OutputText("You have joined the religion: " + ReligionID);
        CurrentFaithLevel = 1;
        AssignNextQuest();
        TextOutputter.Instance.OutputText("Current Quest: " + QuestDescription);
    }

    public virtual void OnLeaveReligion()
    {
        Debug.Log("Left religion: " + ReligionID);
        TextOutputter.Instance.OutputText("You have left the religion: " + ReligionID);
    }

    public virtual void AssignNextQuest()
    {
        // Override in subclasses
    }

    public virtual void CompleteQuest()
    {
        if (CurrentQuestStatus == QuestStatus.ReadyToTurnIn)
        {
            CurrentFaithLevel++;
            TextOutputter.Instance.OutputText($"Faith Level Up! You are now Level {CurrentFaithLevel} in {ReligionID}.");
            GrantLevelReward(CurrentFaithLevel);
            
            if (CurrentFaithLevel < 4)
            {
                AssignNextQuest();
            }
            else
            {
                CurrentQuestStatus = QuestStatus.None;
                QuestDescription = "You have reached the pinnacle of faith.";
            }
        }
    }

    public virtual void GrantLevelReward(int level)
    {
        // Override in subclasses
    }

    public virtual void OnEnemyDefeated(Enemy enemy)
    {
        if (CurrentQuestStatus == QuestStatus.SlayingBoss && enemy is Boss)
        {
            CurrentQuestStatus = QuestStatus.ReadyToTurnIn;
            TextOutputter.Instance.OutputText($"The beast is slain. Return to a religious encounter to claim your reward.");
        }
    }

    public void SetQuestReady()
    {
        CurrentQuestStatus = QuestStatus.ReadyToTurnIn;
    }
}
