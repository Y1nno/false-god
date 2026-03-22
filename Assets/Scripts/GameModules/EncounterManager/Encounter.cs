using UnityEngine;
using System.Collections.Generic;

public abstract class Encounter : Subject, IPromptResponder
{
    protected readonly float _difficulty;

    public Encounter(float difficulty)
    {
        _difficulty = difficulty;
    }
    public virtual void StartEncounter()
    {
        Notify(EventType.EncounterStart);
    }
    public virtual void ResolveEncounter()
    {
        Notify(EventType.EncounterResolve);
    }

    public virtual void ProcessPromptResponse(int decisionIndex)
    {
        RecieveDecision(decisionIndex);
    }

    public abstract void RecieveDecision(int decisionIndex);
}
