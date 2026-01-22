using UnityEngine;

public class Encounter : Subject
{
    private readonly float _difficulty;

    public Encounter(float difficulty)
    {
        _difficulty = difficulty;

        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        // Register DungeonManager as an observer to this encounter for starting and resolving notifications
        AttachObserver(dm);
    }
    public virtual void ProcessPlayerCommand(PlayerCommand command)
    {

    }
    public virtual void StartEncounter()
    {
        Notify(EventType.EncounterStart);
    }
    public virtual void ResolveEncounter()
    {
        Notify(EventType.EncounterResolve);
    }
}
