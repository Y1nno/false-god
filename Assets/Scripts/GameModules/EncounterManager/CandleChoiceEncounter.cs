using UnityEngine;
using System.Collections.Generic;

public class CandleChoiceEncounter : Encounter
{
    private List<EncounterType> _options = new List<EncounterType>();

    public CandleChoiceEncounter(float difficulty) : base(difficulty)
    {
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        GenerateOptions();
        
        List<string> optionStrings = new List<string>();
        foreach (var option in _options)
        {
            optionStrings.Add(option.ToString());
        }
        OutputEnumeratedDecisionOptions(optionStrings, "Candle Rite: Choose your path:");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            EncounterType chosenType = _options[decisionIndex];
            TextOutputter.Instance.OutputText($"You chose: {chosenType}");
            
            // Immediately replace this choice screen with the chosen encounter
            RunManager.Instance.GetService<EncounterManager>().ReplaceCurrentEncounter(chosenType);
        }
        else
        {
            Debug.LogWarning("Invalid decision index for CandleChoiceEncounter");
        }
    }

    private void GenerateOptions()
    {
        _options.Clear();
        List<EncounterType> validTypes = new List<EncounterType>()
        {
            EncounterType.Treasure,
            EncounterType.NPC,
            EncounterType.Rest,
            EncounterType.Religious,
            EncounterType.Enemy,
            EncounterType.Trap
        };

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, validTypes.Count);
            _options.Add(validTypes[randomIndex]);
            validTypes.RemoveAt(randomIndex);
        }
    }
}
