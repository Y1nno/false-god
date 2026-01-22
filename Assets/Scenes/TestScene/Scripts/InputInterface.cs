using UnityEngine;

public class InputInterface : MonoBehaviour
{
    
    public void StartNewRun()
    {
        RunManager.Instance.StartNewRun();
    }

    public void AdvanceDungeonRoom()
    {
        RunManager rm = RunManager.Instance;
        DungeonManager dm = rm.GetService<DungeonManager>();
        dm.AdvanceRoom();
    }
}
