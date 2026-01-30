using UnityEngine;
using System.Collections.Generic;

public abstract class Encounter : Subject
{
    private readonly float _difficulty;

    private List<int> decisionsAvailable = new List<int>();

    public Encounter(float difficulty)
    {
        _difficulty = difficulty;

        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        // Register DungeonManager as an observer to this encounter for starting and resolving notifications
        AttachObserver(dm);
        XPManager xm = RunManager.Instance.GetService<XPManager>();
        AttachObserver(xm);
        ScoreManager sm = RunManager.Instance.GetService<ScoreManager>();
        AttachObserver(sm);
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

    public abstract void RecieveDecision(int decisionIndex);

    public bool ValidateDecisionIndex(int decisionIndex)
    {
        Debug.Log($"Validating decision index: {decisionIndex}");
        Debug.Log($"Decisions available count: {decisionsAvailable.Count}");
        Debug.Log($"Is decision index valid: {decisionIndex >= 0 && decisionIndex < decisionsAvailable.Count}");
        return decisionIndex >= 0 && decisionIndex < decisionsAvailable.Count;
    }

    public void OutputEnumeratedDecisionOptions(List<string> options, string header = "Choose an option:")
    {
        decisionsAvailable.Clear();
        string outputText = $"{header}\n";
        for (int i = 0; i < options.Count; i++)
        {
            outputText += $"{i + 1}. {options[i]}\n";
            decisionsAvailable.Add(i);
        }
        TextOutputter.Instance.OutputText(outputText);
    }
}
