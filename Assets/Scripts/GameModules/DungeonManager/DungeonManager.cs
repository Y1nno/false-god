using UnityEngine;

public class DungeonManager
{
    private int k_maxDungeonFloor = 100;
    private int k_roomsPerFloor = 10;

    public int currentDungeonFloor { get; private set; } = 1;
    public int roomAtCurrentFloor { get; private set; } = 1;

    // Advance to the next room, and if necessary, the next floor
    public DungeonFloorData AdvanceRoom()
    {
        roomAtCurrentFloor++;
        if (roomAtCurrentFloor > k_roomsPerFloor)
        {
            AdvanceFloor();
        }
        return new DungeonFloorData(currentDungeonFloor, roomAtCurrentFloor);
    }

    // Advance to the next floor
    private void AdvanceFloor()
    {
        if (IsDungeonComplete())
        {
            Debug.Log("Dungeon is complete.");
            return;
        }
        currentDungeonFloor++;
        roomAtCurrentFloor = 1;
    }

    // Check if the dungeon run is complete
    public bool IsDungeonComplete()
    {
        return currentDungeonFloor >= k_maxDungeonFloor;
    }
}

// Data structure for dungeon floor information
public struct DungeonFloorData
{
    public int floor;
    public int room;

    public DungeonFloorData(int x, int y)
    {
        floor = x;
        room = y;
    }
}