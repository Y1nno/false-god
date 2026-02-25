using UnityEngine;

[System.Serializable]
public class Quest : IObserver
{
    public int QuestProgress { get; private set; } = 0;
    public readonly int QuestGoal;

    public Quest(int difficultyLevel)
    {
        QuestGoal = DetermineQuestGoal(difficultyLevel);
    }

    public virtual int DetermineQuestGoal(int difficultyLevel)
    {
        return difficultyLevel * 10;
    }

    public void ProgressQuest(int progressAmount)
    {
        QuestProgress += progressAmount;
        if (QuestProgress >= QuestGoal)
        {
            OnCompleteQuest();
        }
    }

    public virtual void AttachDefaultSubjects(){}

    public virtual void OnAcceptQuest(){}
    public virtual void OnCompleteQuest(){}
}

public enum QuestType
{
    JoinReligion,
    
}