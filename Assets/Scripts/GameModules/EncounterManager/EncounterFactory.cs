using UnityEngine;

public class EncounterFactory
{
    private int _floor;
    private int _room;
    
    public static Encounter CreateEncounter(DungeonFloorData floorData, EncounterType type)
    {
        _floor = floorData.Floor;
        _room = floorData.Room;

        float difficulty = CalculateEncounterDifficulty();
    }
    
    private float CalculateEncounterDifficulty()
    {
        float baseDifficulty = _floor + _room * 1.5f;

        return Math.RandomRange(baseDifficulty * 0.9f, baseDifficulty * 1.1f);
    }
}
