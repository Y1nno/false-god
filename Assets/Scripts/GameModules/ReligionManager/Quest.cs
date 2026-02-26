using System;
using UnityEngine;

[Serializable]
public class Quest : IObserver
{
    [SerializeField] protected int questProgress = 0;
    [SerializeField] protected int questGoal = 0;
    [SerializeField] protected QuestType questType;

    public int QuestProgress => questProgress;
    public int QuestGoal => questGoal;
    public QuestType QuestType => questType;

    public void ProgressQuest(int progressAmount)
    {
        questProgress += progressAmount;
        if (questProgress >= questGoal)
            OnCompleteQuest();
    }

    public virtual void AttachDefaultSubjects() {}

    public virtual void OnAcceptQuest() {}
    public virtual void OnCompleteQuest() {}
}

