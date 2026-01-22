using UnityEngine;
using System.Collections.Generic;

public class EncounterManager
{
    private Encounter _currentEncounter; 

    private float _treasureEncounterChance = 0.0f;
    private float k_treasureEncounterIncrement = 0.015f;
    private float _trapEncounterChance = 0.0f;
    private float k_trapEncounterIncrement = 0.025f;

    private DungeonManager _dm = RunManager.Instance.GetService<DungeonManager>();

    
    #region Public API 
    public void CreateEncounter()
    {
        EncounterType encounterType = DetermineEncounterType();
        HandleEncounterChances(encounterType);
        _currentEncounter = EncounterFactory.CreateEncounter(encounterType);
        
    }

    public void BeginEncounter()
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        _currentEncounter.StartEncounter();
    }

    public void SubmitPlayerCommand(PlayerCommand command)
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        _currentEncounter.ProcessPlayerCommand(command);
    }

    public void ResolveEncounter()
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        _currentEncounter.ResolveEncounter();
        _currentEncounter = null;
    }
    #endregion

    private EncounterType DetermineEncounterType()
    {
        DungeonFloorData floorData = _dm.GetCurrentDungeonFloorData();
        int floor = floorData.Floor;
        int room = floorData.Room;

        //Every 20 encounters is a religious encounter
        if (floor % 2 == 0 && room == 10)
        {
            return EncounterType.Religious;
        }
        // Every 10 encounters is an NPC encounter
        else if (room == 10)
        {
            return EncounterType.NPC;
        }
        // The last room on the floor is always a rest encounter
        else if (room % 11 ==0)
        {
            return EncounterType.Rest;
        }
        else if (Random.NextDouble() <= _trapEncounterChance)
        {
            return EncounterType.Trap;
        }
        else if (Random.NextDouble() <= _treasureEncounterChance)
        {
            return EncounterType.Treasure;
        }
        else
        {
            return EncounterType.Enemy;
        }

        return possibleEncounters[index];
    }

    private void HandleEncounterChances(EncounterType encounterType)
    {
        switch (encounterType)
        {
            case EncounterType.Treasure:
                _treasureEncounterChance = 0.0f;
                _trapEncounterChance += k_trapEncounterIncrement;
                break;
            case EncounterType.Trap:
                _trapEncounterChance = 0.0f;
                _treasureEncounterChance += k_treasureEncounterIncrement;
                break;
            default:
                _treasureEncounterChance += k_treasureEncounterIncrement;
                _trapEncounterChance += k_trapEncounterIncrement;
                break;
        }
    }
}

public enum EncounterType
{
    Enemy,
    Treasure,
    Trap,
    NPC,
    Rest,
    Religious
}
