using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Religion", menuName = "Scriptable Objects/Religion/Religion")]
public class ReligionSO : ScriptableObject
{
    public string ReligionName;
    public string Description;
    public Sprite Icon;

    public List<ReligionLevel> levels;

    public ReligionData ConvertToReligion()
    {
        ReligionData result = new ReligionData
        {
            ReligionName = this.ReligionName,
            Description = this.Description,
            Icon = this.Icon,
            Levels = this.levels
        };
        return result;
    }

}

[Serializable]
public class ReligionLevel
{
    public QuestDataSO quest;
    public ReligionRewardSO reward;
}