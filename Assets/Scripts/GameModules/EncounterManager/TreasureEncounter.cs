using UnityEngine;

public class TreasureEncounter : Encounter
{
    public TreasureEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"TreasureEncounter created with difficulty: {difficulty}");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            // Process the decision made by the player

        }
        else
        {
            Debug.LogWarning("Invalid decision index received in TreasureEncounter.");
        }
    }
}
