using UnityEngine;

public class RestEncounter : Encounter
{
    public RestEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"RestEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            // Process the decision made by the player

        }
        else
        {
            Debug.LogWarning("Invalid decision index received in RestEncounter.");
        }
    }
}
