using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrapEncounter", menuName = "Encounters/Trap Encounter")]
public class TrapEncounterSO : ScriptableObject
{
    [TextArea(3, 10)]
    public string Description;

    public List<TrapChoice> Choices = new List<TrapChoice>();
}

[Serializable]
public class TrapChoice
{
    public Stat StatToRoll;
    [Range(1, 10)]
    public int RelativeDifficulty;
    public int PassDamage;
    public int FailDamage;
}
