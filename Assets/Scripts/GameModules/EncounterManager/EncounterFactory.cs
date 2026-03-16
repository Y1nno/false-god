using UnityEngine;
using System;

public class EncounterFactory
{
    private int _floor;
    private int _room;
    
    // Create an encounter based on dungeon floor data and encounter type
    public Encounter CreateEncounter(DungeonFloorData floorData, EncounterType type, TrapEncounterSO trapSO = null)
    {
        _floor = floorData.Floor;
        _room = floorData.Room;

        float difficulty = CalculateEncounterDifficulty();
        switch (type)
        {
            case EncounterType.Enemy:
                return new EnemyEncounter(difficulty);
            case EncounterType.Treasure:
                return new TreasureEncounter(difficulty);
            case EncounterType.Trap:
                return new TrapEncounter(difficulty, trapSO, _floor);
            case EncounterType.NPC:
                return new NPCEncounter(difficulty);
            case EncounterType.Religious:
                return new ReligiousEncounter(difficulty);
            case EncounterType.Rest:
                return new RestEncounter(difficulty);
            case EncounterType.Blacksmith:
                return new BlacksmithEncounter(difficulty);
            case EncounterType.Merchant:
                return new MerchantEncounter(difficulty);
            case EncounterType.Portal:
                return new PortalEncounter(difficulty);
            case EncounterType.Boss:
                return new BossEncounter(difficulty);
            default:
                throw new ArgumentOutOfRangeException("Invalid encounter type.");
        }
    }
    
    // Calculate the difficulty of the encounter based on floor and room
    private float CalculateEncounterDifficulty()
    {
        float baseDifficulty = _floor + _room * 1.5f;

        return UnityEngine.Random.Range(baseDifficulty * 0.9f, baseDifficulty * 1.1f);
    }
}
