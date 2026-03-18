using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnWeight
{
    public EnemySO enemyData;
    public int weight = 1;
}

[CreateAssetMenu(fileName = "DepthTable", menuName = "Scriptable Objects/Combat/Depth Encounter Table")]
public class DepthEncounterTableSO : ScriptableObject
{
    [Header("Depth Range (Inclusive)")]
    public int MinDepth;
    public int MaxDepth;
    
    [Header("Enemy Level Range (Inclusive)")]
    public int MinLevel;
    public int MaxLevel;
    
    [Header("Spawn Counts (%)")]
    public float OneEnemyChance = 50f;
    public float TwoEnemyChance = 30f;
    public float ThreeEnemyChance = 20f;

    [Header("Enemy Pool")]
    public List<EnemySpawnWeight> EnemyPool;
}
