using UnityEngine;
using System.Collections.Generic;

public class Religion
{
    public string ReligionName { get; protected set; }
    public string Description { get; protected set; }
    public Sprite icon { get; protected set; }
    public List<ReligionLevel> levels { get; protected set; }
    public int FaithLevel { get; protected set; } = 0;

    public Religion(ReligionData religionData)
    {
        ReligionName = religionData.ReligionName;
        Description = religionData.Description;
        icon = religionData.Icon;
        levels = religionData.Levels;
    }

    public virtual void OnJoinReligion()
    {
        Debug.Log("Joined religion: " + ReligionName);
        TextOutputter.Instance.OutputText("You have joined the religion: " + ReligionName);
        CompleteCurrentQuest();
    }

    public virtual void OnLeaveReligion()
    {
        Debug.Log("Left religion: " + ReligionName);
        TextOutputter.Instance.OutputText("You have left the religion: " + ReligionName);
    }

    public void CompleteCurrentQuest()
    {
        FaithLevel++;
        levels[FaithLevel].reward.ApplyReward();
        TextOutputter.Instance.OutputText("You have completed the current quest for the religion: " + ReligionName);
    }

    public bool CanPlayerCompleteQuest()
    {
        return levels[FaithLevel].quest.CanComplete();
    }

    public bool TryCompleteCurrentQuest()
    {
        if (CanPlayerCompleteQuest())
        {
            CompleteCurrentQuest();
            return true;
        }
        return false;
    }
}

public struct ReligionData
{
    public string ReligionName;
    public string Description;
    public Sprite Icon;

    public List<ReligionLevel> Levels;
}