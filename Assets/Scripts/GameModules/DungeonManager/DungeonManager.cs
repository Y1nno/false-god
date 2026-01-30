using UnityEngine;

public class DungeonManager : GameModule, IObserver
{
    private int k_maxDungeonFloor = 100;
    private int k_roomsPerFloor = 11; // 10 rooms + 1 rest room

    public int CurrentDungeonFloor { get; private set; } = 1;
    public int RoomAtCurrentFloor { get; private set; } = 1;

    private EncounterManager _em = null;

    public DungeonManager(){}

    public override void AttachDefaultObservers()
    {
        _em = RunManager.Instance.GetService<EncounterManager>();
        AttachObserver(_em);
    }

    // Advance to the next room, and if necessary, the next floor
    public void AdvanceRoom()
    {
        RoomAtCurrentFloor++;
        if (RoomAtCurrentFloor > k_roomsPerFloor)
        {
            AdvanceFloor();
        }
        TextOutputter.Instance.OutputText($"Advanced to floor {CurrentDungeonFloor}, room {RoomAtCurrentFloor}.");
        if (_em == null)
        {
            AttachDefaultObservers();
        }
        Notify(EventType.DungeonRoomAdvance);
    }

    // Get the current dungeon floor data
    public DungeonFloorData GetCurrentDungeonFloorData()
    {
        return new DungeonFloorData(CurrentDungeonFloor, RoomAtCurrentFloor);
    }

    // Advance to the next floor
    private void AdvanceFloor()
    {
        if (IsDungeonComplete())
        {
            TextOutputter.Instance.OutputText("Run complete, cannot advance further.");
            return;
        }
        CurrentDungeonFloor++;
        RoomAtCurrentFloor = 1;
        Notify(EventType.DungeonFloorAdvance);
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
}

// Data structure for dungeon floor information
public struct DungeonFloorData
{
    public int Floor;
    public int Room;

    public DungeonFloorData(int floor, int room)
    {
        Floor = floor;
        Room = room;
    }
}