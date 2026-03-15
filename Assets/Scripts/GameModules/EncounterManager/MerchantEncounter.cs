using UnityEngine;

public class MerchantEncounter : Encounter
{
    public MerchantEncounter(float difficulty) : base(difficulty)
    {
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("You encounter a Wandering Merchant. He has rare items for sale.");
        // TODO: Implement actual merchant logic/UI
    }

    public override void RecieveDecision(int decisionIndex)
    {
        // For now, any decision just resolves the encounter
        ResolveEncounter();
    }
}
