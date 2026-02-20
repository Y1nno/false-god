using UnityEngine;
using System.Collections.Generic;

public class CandleChoiceEncounter : Encounter, IPromptResponder
{
    private List<EncounterType> _options = new List<EncounterType>();
    private int k_AmountOfOptions = 3;

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
        Prompt prompt = new Prompt("Candle Rite: Choose your path:", optionStrings, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        EncounterType chosenType = _options[decisionIndex];
        TextOutputter.Instance.OutputText($"You chose: {chosenType}");

        // Immediately replace this choice screen with the chosen encounter
        RunManager.Instance.GetService<EncounterManager>().ReplaceCurrentEncounter(chosenType);

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

        for (int i = 0; i < k_AmountOfOptions; i++)
        {
            int randomIndex = Random.Range(0, validTypes.Count);
            _options.Add(validTypes[randomIndex]);
            validTypes.RemoveAt(randomIndex);
        }
    }
}
