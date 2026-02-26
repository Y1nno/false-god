using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Religion", menuName = "Scriptable Objects/Religion/Religion")]
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
    public QuestDataSO quest;
    public ReligionRewardSO reward;
}