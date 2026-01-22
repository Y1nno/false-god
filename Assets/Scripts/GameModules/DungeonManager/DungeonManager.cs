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
        TextOutputter.Instance.OutputText($"Advanced to floor {currentDungeonFloor}, room {roomAtCurrentFloor}.");
        return new DungeonFloorData(currentDungeonFloor, roomAtCurrentFloor);
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