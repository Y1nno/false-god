using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrapEncounter", menuName = "Encounters/Trap Encounter")]
public class TrapEncounterSO : ScriptableObject
{
    public string TrapName;
    [TextArea(3, 10)]
    public string Description;

    [Header("Base Requirements")]
    public int BaseOvercomeReq;
    public int BaseDodgeReq;
    public int BaseDismantleReq;

    [Header("Base Rewards/Consequences")]
    public int BaseDamage;
    public int BaseExp;

    [Header("Spawn Settings")]
    [Range(0, 100)]
    public float SpawnChance;
}
