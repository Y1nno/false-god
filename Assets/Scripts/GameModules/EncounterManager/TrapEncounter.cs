using UnityEngine;

public class TrapEncounter : Encounter
{
    public TrapEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"TrapEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        ResolveEncounter();
    }
}
