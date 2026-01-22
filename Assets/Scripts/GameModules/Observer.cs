using UnityEngine;

public class Observer
{
    // Notification method called when an observed subject triggers an event
    public virtual void OnNotify(object subject, EventType eventType){}
}

public enum EventType
{
    PlayerDeath,
    EncounterStart,
    EncounterResolve
}
