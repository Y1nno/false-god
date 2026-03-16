using UnityEngine;
using System.Collections.Generic;

public class PortalEncounter : Encounter
{
    private List<EncounterType> _options = new List<EncounterType> { EncounterType.Blacksmith, EncounterType.Rest, EncounterType.Merchant };

    public PortalEncounter(float difficulty) : base(difficulty)
    {
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("The Scroll of Portal creates a shimmering gateway...");
        
        List<string> optionNames = new List<string> { "Go to Blacksmith", "Go to Rest Site", "Go to Merchant" };
        Prompt prompt = new Prompt("Choose your destination:", optionNames, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (decisionIndex >= 0 && decisionIndex < _options.Count)
        {
            EncounterType selected = _options[decisionIndex];
            TextOutputter.Instance.OutputText($"Entering the portal towards: {selected}...");
            
            EncounterManager em = RunManager.Instance.GetService<EncounterManager>();
            if (em != null)
            {
                em.ReplaceCurrentEncounter(selected);
            }
        }
        else
        {
            // Invalid selection, just resolve to avoid stuck state
            ResolveEncounter();
        }
    }
}
