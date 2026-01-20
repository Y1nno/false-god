using UnityEngine;

public class Observer
{
    // Notification method called when an observed subject triggers an event
    public virtual void OnNotify(object subject, string eventType){}
}

public enum EventType
{
    PlayerDeath
}
