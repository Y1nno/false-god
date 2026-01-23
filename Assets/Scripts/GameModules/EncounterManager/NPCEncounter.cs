using UnityEngine;

public class NPCEncounter : Encounter
{
    public NPCEncounter(float difficulty) : base(difficulty)
    {
        Debug.Log($"NPCEncounter created with difficulty: {difficulty}");
    }
}
