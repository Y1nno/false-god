using UnityEngine;

public class RestEncounter : Encounter
{
    public RestEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"RestEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        ResolveEncounter();
    }
}
