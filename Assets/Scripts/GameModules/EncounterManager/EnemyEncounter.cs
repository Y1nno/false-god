using UnityEngine;

public class EnemyEncounter : Encounter
{
    public EnemyEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"EnemyEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            // Process the decision made by the player

        }
        else
        {
            Debug.LogWarning("Invalid decision index received in EnemyEncounter.");
        }
    }
}
