using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnWeight
{
    public EnemySO enemyData;
    public int weight = 1;
}

[System.Serializable]
public class SpawnPoolConfig
{
    [Header("Pool Config")]
    [Tooltip("The chance this pool is selected when spawning an enemy")]
    public float SpawnChance;
    
    [Header("Level Range")]
    public int MinLevel;
    public int MaxLevel;
    
    [Header("Enemies In Pool")]
    public float UniqueChance;
    public List<EnemySpawnWeight> Enemies;
}

[CreateAssetMenu(fileName = "DepthTable", menuName = "Scriptable Objects/Combat/Depth Encounter Table")]
public class DepthEncounterTableSO : ScriptableObject
{
    [Header("Depth Range (Inclusive)")]
    public int MinDepth;
    public int MaxDepth;
    
    [Header("Spawn Counts (%)")]
    public float OneEnemyChance = 50f;
    public float TwoEnemyChance = 30f;
    public float ThreeEnemyChance = 20f;

    [Header("Spawn Pools")]
    public List<SpawnPoolConfig> Pools;
}
