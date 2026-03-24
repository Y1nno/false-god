using UnityEngine;
using System.Collections.Generic;

public class ShrineEncounter : Encounter
{
    public ShrineEncounter(float difficulty) : base(difficulty) { }

    public override void StartEncounter()
    {
        base.StartEncounter();
        List<string> options = new List<string> { "Heal (50% HP)", "Leave" };
        new Prompt("A serene Health Shrine stands before you. What do you do?", options, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (decisionIndex == 0) // Heal
        {
            CombatManager cm = RunManager.Instance.GetService<CombatManager>();
            if (cm != null && cm.Pcm != null)
            {
                Combatant player = cm.Pcm;
                int healAmount = Mathf.RoundToInt(player.GetHealth().MaxValue * 0.5f);
                player.Heal(healAmount);
                TextOutputter.Instance.OutputText($"The shrine's light washes over you, healing {healAmount} HP.");
            }
        }
        else
        {
            TextOutputter.Instance.OutputText("You leave the shrine behind.");
        }
        ResolveEncounter();
    }
}
