using UnityEngine;
using System.Collections.Generic;

public class ReligiousEncounter : Encounter, IPromptResponder
{

    private ReligionManager _rm;
    private List<ReligionSO> _offeredReligions = new List<ReligionSO>();
    public ReligiousEncounter(float difficulty) : base(difficulty)
    {
        _rm = RunManager.Instance.GetService<ReligionManager>();
    }

    public override void StartEncounter()
    {
        Notify(EventType.EncounterStart);

        if (_rm.CurrentReligion == null)
        {
            RiteManager riteManager = RunManager.Instance.GetService<RiteManager>();
            if (riteManager != null && riteManager.HasRite<FaithlessRite>())
            {
                TextOutputter.Instance.OutputText("You cant join a religion because you have the faithless rite");
                ResolveEncounter();
                return;
            }

            OfferReligionChoices();
        }
        else
        {
            OfferCurrentReligionRequirement();
        }
    }

    private void OfferReligionChoices()
    {
        _offeredReligions = _rm.GenerateAvailableReligions();
        List<string> outputOptions = new List<string>();
        for (int i = 0; i < _offeredReligions.Count; i++)
        {
            outputOptions.Add(_offeredReligions[i].ReligionName);
        }
        Prompt prompt = new Prompt("Choose a new religion:", outputOptions, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        Religion currentReligion = _rm.CurrentReligion;
        if (_offeredReligions.Count > 0)
        {
            ReligionSO chosenReligion = _offeredReligions[decisionIndex];
            _offeredReligions = new List<ReligionSO>();
            _rm.JoinReligion(new Religion(chosenReligion.ConvertToReligion()));
        }
        else if (decisionIndex == 0 && currentReligion.TryCompleteCurrentQuest())
        {
            currentReligion.CompleteCurrentQuest();
            TextOutputter.Instance.OutputText("You fulfilled the requirement and received the reward.");
        }

        ResolveEncounter();
    }

    private void OfferCurrentReligionRequirement()
    {
        Religion currentReligion = _rm.CurrentReligion;
        TextOutputter.Instance.OutputText($"Religious requirement: {currentReligion.levels[currentReligion.FaithLevel].quest.GetCurrentRequirementDescription()}");

        if (!currentReligion.CanPlayerCompleteQuest())
        {
            TextOutputter.Instance.OutputText("You cannot fulfill this requirement yet.");
            return;
        }
        Prompt prompt = new Prompt("Do you want to complete the current quest?", new List<string> { "Yes", "No" }, this);
    }
}
