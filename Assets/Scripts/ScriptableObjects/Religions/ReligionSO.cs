using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Religion", menuName = "Scriptable Objects/Religion")]
public class ReligionSO : ScriptableObject
{
    public string religionName;
    public string description;
    public Sprite icon;

    public List<ReligionLevels> levels;

}

[System.Serializable]
public class ReligionLevels
{
    public QuestType requirement;
    public ReligionRewardType reward;
}
