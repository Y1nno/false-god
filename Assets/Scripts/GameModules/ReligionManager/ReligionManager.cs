using UnityEngine;
using System.Collections.Generic;

public class ReligionManager : GameModule
{
    public Religion CurrentReligion { get; private set; } = null;
    private QuestFactory _questFactory = new QuestFactory();
    private readonly List<Religion> _religionsPool = new List<Religion>()
    {
        new ExampleReligion1(),
        new ExampleReligion2(),
        new ExampleReligion3(),
    };

    public List<Quest> ActiveReligionQuests { get; private set; } = new List<Quest>();

    private const int MaxReligionsWhenChoosing = 3;

    public override void AttachDefaultObservers()
    {
        // none for now
    }

    public void JoinReligion(Religion newReligion)
    {
        if (CurrentReligion != null)
        {
            LeaveReligion();
        }
        CurrentReligion = newReligion;
        CurrentReligion.OnJoinReligion();
    }

    public void LeaveReligion()
    {
        if (CurrentReligion == null)
        {
            Debug.LogWarning("No religion to leave.");
            return;
        }
        CurrentReligion.OnLeaveReligion();
        CurrentReligion = null;
    }

    public void AcceptReligionQuest(Quest newQuest)
    {
        if (CurrentReligion == null)
        {
            Debug.LogWarning("Cannot accept religion quest without being in a religion.");
            return;
        }
        ActiveReligionQuests.Add(newQuest);
    }

    public Quest CreateReligionQuest(int questId = 0, int difficultyLevel = 0)
    {
        return _questFactory.CreateQuest(questId, difficultyLevel);
    }

    public List<Religion> GenerateAvailableReligions()
    {
        var availableReligions = new List<Religion>();

        if (_religionsPool == null || _religionsPool.Count == 0)
            return availableReligions;

        int targetCount = Mathf.Min(MaxReligionsWhenChoosing, _religionsPool.Count);

        int safety = 0;
        int maxAttempts = _religionsPool.Count * 10; // prevents infinite loop

        while (availableReligions.Count < targetCount && safety++ < maxAttempts)
        {
            int index = Random.Range(0, _religionsPool.Count);
            Religion religion = _religionsPool[index];

            if (!availableReligions.Contains(religion))
                availableReligions.Add(religion);
        }

        return availableReligions;
    }

}
