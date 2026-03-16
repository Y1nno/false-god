using UnityEngine;

public class BlacksmithEncounter : Encounter
{
    public BlacksmithEncounter(float difficulty) : base(difficulty)
    {
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("You encounter a Blacksmith. He can repair and upgrade your equipment.");
        // TODO: Implement actual blacksmith logic/UI
    }

    public override void RecieveDecision(int decisionIndex)
    {
        // For now, any decision just resolves the encounter
        ResolveEncounter();
    }
}
