using UnityEngine;

// Observer that ends the run when the player dies
public class RunEnder : IObserver
{
    public RunEnder()
    {
        // Subscribe to player death events
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        pm.AttachObserver(this);
    }
    
    public void OnNotify(object subject, EventType et)
    {
        if (et == EventType.PlayerDeath)
        {
            RunManager.Instance.EndRun();
        }
    }
}
