using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Combat/EnemyData")]
public class EnemySO : ScriptableObject
{
    public string EnemyName;
    public int BaseHealth;
    public int BaseMana;

    [Header("Base Primary Stats")]
    public int BaseAtk;
    public int BaseSpAtk;
    public int BaseDef;
    public int BaseSpDef;
    public int BaseSpeed;

    [Header("Base Secondary Stats")]
    public float DodgeChance;
    public float CritChance;

    [Header("Reward")]
    public int BaseXP;

    [Header("Actions")]
    public List<int> AvailableActionIDs;

    [Header("Combat Defaults")]
    public int GoldValue = 0;

    [Header("Misc")]
    [TextArea]
    public string SpecialDescription;
}
