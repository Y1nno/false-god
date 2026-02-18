using UnityEngine;
using System.Collections.Generic;

public class ReligiousEncounter : Encounter
{

    private ReligionManager _rm;
    private List<Religion> _offeredReligions = new List<Religion>();
    private List<Quest> _offeredQuests = new List<Quest>();
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
            // TODO: Implement religious encounter options for existing religion members
        }
    }

    private void OfferReligionChoices()
    {
        _offeredReligions = _rm.GenerateAvailableReligions();
        List<string> outputOptions = new List<string>();
        for (int i = 0; i < _offeredReligions.Count; i++)
        {
            outputOptions.Add(_offeredReligions[i].ReligionID);
        }
        OutputEnumeratedDecisionOptions(outputOptions, "Choose a new religion:");
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            if (_offeredReligions.Count == 0 && _offeredQuests.Count > 0)
            {
                Quest chosenQuest = _offeredQuests[decisionIndex];
                _offeredQuests = new List<Quest>();
                _rm.AcceptReligionQuest(chosenQuest);
            }
            else if (_offeredQuests.Count == 0 && _offeredReligions.Count > 0)
            {
                Religion chosenReligion = _offeredReligions[decisionIndex];
                _offeredReligions = new List<Religion>();
                _rm.JoinReligion(chosenReligion);
            }
            else
            {
                Debug.LogWarning("ReligiousEncounter received decision but no valid options are available.");
            }
            
            ResolveEncounter();
        }
        else
        {
            Debug.LogWarning("Invalid decision index received in ReligiousEncounter.");
        }
    }
}
