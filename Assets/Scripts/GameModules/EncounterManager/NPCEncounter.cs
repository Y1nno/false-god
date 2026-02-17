using UnityEngine;

public class NPCEncounter : Encounter
{
    public NPCEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"NPCEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            // Process the decision made by the player

            ResolveEncounter();
        }
        else
        {
            Debug.LogWarning("Invalid decision index received in NPCEncounter.");
        }
    }
}
