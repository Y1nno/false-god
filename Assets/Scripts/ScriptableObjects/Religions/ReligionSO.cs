using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Religion", menuName = "Scriptable Objects/Religion")]
public class ReligionSO : ScriptableObject
{
    public string religionName;
    public string description;
    public Sprite icon;

    public List<ReligionLevel> levels;

}

[Serializable]
public class ReligionLevel
{
    // Requirement (Quest)
    public QuestType requirement;

    // Requirement params (fill based on requirement)
    public string requiredReligionName; // for JoinReligion (swap to ReligionSO if you have it)
    public int requiredItemID;          // for GiveItem
    public int requiredItemAmount;      // for GiveItem
    public int bossID;                  // for KillBoss
    public ReligionRewardSO reward;
}