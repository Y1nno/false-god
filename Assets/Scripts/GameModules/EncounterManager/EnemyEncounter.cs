using UnityEngine;

public class EnemyEncounter : Encounter
{
    public EnemyEncounter(float difficulty) : base(difficulty)
    {
        Debug.Log($"EnemyEncounter created with difficulty: {difficulty}");
    }
}
