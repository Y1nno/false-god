using UnityEngine;
using System.Collections.Generic;

public class RestEncounter : Encounter
{
    public RestEncounter(float difficulty) : base(difficulty)
    {
        //Debug.Log($"RestEncounter created with difficulty: {difficulty}");
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("You found a safe spot to rest.");
        
        List<string> choices = new List<string> { "Rest (Restore 35% HP)", "Leave" };
        new Prompt("What would you like to do?", choices, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (decisionIndex == 0)
        {
            PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
            int healAmount = Mathf.RoundToInt(pm.Health.MaxValue * 0.35f);
            pm.Heal(healAmount);
            TextOutputter.Instance.OutputText($"You rested and recovered {healAmount} HP.");
        }
        
        ResolveEncounter();
    }
}
