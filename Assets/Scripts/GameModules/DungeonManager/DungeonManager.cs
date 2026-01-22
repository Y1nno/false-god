using UnityEngine;

public class DungeonManager
{
    private int k_maxDungeonFloor = 100;
    private int k_roomsPerFloor = 11; // 10 rooms + 1 rest room

    public int CurrentDungeonFloor { get; private set; } = 1;
    public int RoomAtCurrentFloor { get; private set; } = 1;

    // Advance to the next room, and if necessary, the next floor
    public DungeonFloorData AdvanceRoom()
    {
        RoomAtCurrentFloor++;
        if (RoomAtCurrentFloor > k_roomsPerFloor)
        {
            AdvanceFloor();
        }
        TextOutputter.Instance.OutputText($"Advanced to floor {currentDungeonFloor}, room {roomAtCurrentFloor}.");
        return new DungeonFloorData(currentDungeonFloor, roomAtCurrentFloor);
    }

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
        currentDungeonFloor++;
        roomAtCurrentFloor = 1;
        TextOutputter.Instance.OutputText($"Advanced to floor {currentDungeonFloor}, room {roomAtCurrentFloor}.");
    }
    

    // Check if the dungeon run is complete
    public bool IsDungeonComplete()
    {
        return CurrentDungeonFloor >= k_maxDungeonFloor;
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