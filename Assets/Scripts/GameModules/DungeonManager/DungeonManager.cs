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
        return new DungeonFloorData(CurrentDungeonFloor, RoomAtCurrentFloor);
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
            Debug.Log("Dungeon is complete.");
            return;
        }
        CurrentDungeonFloor++;
        RoomAtCurrentFloor = 1;
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