using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/Combat/BossData")]
public class BossSO : ScriptableObject
{
    public string BossName;
    public int BaseHP;
    public int BaseMana;

    [Header("Stats")]
    public int STR;
    public int DEX;
    public int INT;
    public int SPD;

    [Header("Secondary Stats")]
    public float DodgeChance;
    public float CritChance;

    [Header("Reward")]
    public int XP;

    [Header("Loot")]
    public List<string> DropIDs;

    [Header("Actions")]
    public int SpecialActionID;

    [Header("Misc")]
    public string PassiveDescription;
}
