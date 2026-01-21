using UnityEngine;

// Observer that ends the run when the player dies
public class RunEnder : Observer
{
    public RunEnder()
    {
        // Subscribe to player death events
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        pm.AttachObserver(this);
    }
    
    public override void OnNotify(object subject, EventType et)
    {
        if (et == EventType.PlayerDeath)
        {
            RunManager.Instance.EndRun();
        }
    }
}
