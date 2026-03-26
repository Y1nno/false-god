using UnityEngine;

public class DungeonManager : GameModule, IObserver
{
    private int k_maxDungeonFloor = 100;

    public int CurrentDungeonFloor { get; private set; } = 1;
    public int CurrentEncounterIndex { get; private set; } = 1;
    public int EncountersInCurrentFloor { get; private set; } = 1;
    public RoomSize CurrentRoomSize { get; private set; }
    public int HighestFloorReached { get; private set; } = 1;

    private EncounterManager _em = null;

    public DungeonManager(){}

    public override void AttachDefaultObservers()
    {
        _em = RunManager.Instance.GetService<EncounterManager>();
        _em.AttachObserver(this); // Listen for resolution to advance room
        AttachObserver(_em);      // Let EM listen to room advances
        InitializeFloor();
    }

    private void InitializeFloor()
    {
        CurrentRoomSize = RollRoomSize(CurrentDungeonFloor);
        EncountersInCurrentFloor = GetEncountersForSize(CurrentRoomSize);
        CurrentEncounterIndex = 1;
    }

    public void RestoreState(int floor, int encounterIndex, RoomSize size, int totalEncounters)
    {
        CurrentDungeonFloor = floor;
        CurrentRoomSize = size;
        EncountersInCurrentFloor = totalEncounters;
        CurrentEncounterIndex = encounterIndex;
        if (floor > HighestFloorReached) HighestFloorReached = floor;
    }

    public void RestoreMetaState(int highestFloor)
    {
        HighestFloorReached = highestFloor;
    }

    // Advance to the next encounter or floor
    public void AdvanceRoom()
    {
        CurrentEncounterIndex++;
        if (CurrentEncounterIndex > EncountersInCurrentFloor)
        {
            AdvanceFloor();
        }
        else
        {
            Debug.Log($"Floor {CurrentDungeonFloor}: Encounter {CurrentEncounterIndex}/{EncountersInCurrentFloor} ({CurrentRoomSize})");
            Notify(EventType.DungeonEncounterAdvance);
        }

        if (_em == null)
        {
            AttachDefaultObservers();
        }
    }

    // Get the current dungeon floor data (index is now the encounter index)
    public DungeonFloorData GetCurrentDungeonFloorData()
    {
        return new DungeonFloorData(CurrentDungeonFloor, CurrentEncounterIndex, EncountersInCurrentFloor);
    }

    // Advance to the next floor (Level/Depth)
    private void AdvanceFloor()
    {
        if (IsDungeonComplete())
        {
            TextOutputter.Instance.OutputText("Run complete, cannot advance further.");
            return;
        }
        CurrentDungeonFloor++;
        if (CurrentDungeonFloor > HighestFloorReached) HighestFloorReached = CurrentDungeonFloor;
        InitializeFloor();
        
        TextOutputter.Instance.OutputText($"--- Advanced to Floor {CurrentDungeonFloor} ---");
        TextOutputter.Instance.OutputText($"Room Size: {CurrentRoomSize} ({EncountersInCurrentFloor} encounters)");
        
        Notify(EventType.DungeonFloorAdvance);
        Notify(EventType.DungeonRoomAdvance); // New floor counts as a new room/encounter set
    }

    // Check if the dungeon run is complete
    public bool IsDungeonComplete()
    {
        return CurrentDungeonFloor >= k_maxDungeonFloor;
    }

    // Handle notifications from observed subjects
    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EncounterResolve)
        {
            // Advance room on encounter resolution
            AdvanceRoom();
        }
    }
    private RoomSize RollRoomSize(int depth)
    {
        float roll = Random.Range(0f, 100f);
        if (depth <= 10)
        {
            if (roll < 40) return RoomSize.Small;
            if (roll < 80) return RoomSize.Medium;
            if (roll < 95) return RoomSize.Large;
            return RoomSize.ExtraLarge;
        }
        else if (depth <= 20)
        {
            if (roll < 30) return RoomSize.Small;
            if (roll < 70) return RoomSize.Medium;
            if (roll < 90) return RoomSize.Large;
            return RoomSize.ExtraLarge;
        }
        else if (depth <= 30)
        {
            if (roll < 20) return RoomSize.Small;
            if (roll < 50) return RoomSize.Medium;
            if (roll < 85) return RoomSize.Large;
            return RoomSize.ExtraLarge;
        }
        else // 31+
        {
            if (roll < 5) return RoomSize.Small;
            if (roll < 30) return RoomSize.Medium;
            if (roll < 80) return RoomSize.Large;
            return RoomSize.ExtraLarge;
        }
    }

    private int GetEncountersForSize(RoomSize size)
    {
        return size switch
        {
            RoomSize.Small => Random.Range(3, 6),
            RoomSize.Medium => Random.Range(5, 9),
            RoomSize.Large => Random.Range(8, 13),
            RoomSize.ExtraLarge => Random.Range(12, 16),
            _ => 1
        };
    }
}

public enum RoomSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

// Data structure for dungeon floor information
public struct DungeonFloorData
{
    public int Floor;
    public int Room; // Now used as EncounterIndex within floor
    public int TotalEncountersInRoom;

    public DungeonFloorData(int floor, int room, int total)
    {
        Floor = floor;
        Room = room;
        TotalEncountersInRoom = total;
    }
}