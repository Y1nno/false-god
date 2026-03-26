using UnityEngine;
using System.Collections.Generic;

public class ReligiousEncounter : Encounter, IPromptResponder
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
        base.StartEncounter();

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
            ShowMemberInteraction();
        }
    }

    private void OfferReligionChoices()
    {
        _offeredReligions = _rm.GenerateAvailableReligions();
        List<string> outputOptions = new List<string>();
        for (int i = 0; i < _offeredReligions.Count; i++)
        {
            outputOptions.Add("Join " + _offeredReligions[i].ReligionID);
        }
        outputOptions.Add("Leave"); // The Decline option
        new Prompt("A group of mystics offer you a path of faith. Choose a new religion:", outputOptions, this);
    }

    private void ShowMemberInteraction()
    {
        Religion r = _rm.CurrentReligion;
        string msg = $"Welcome, follower of {r.ReligionID}.\nFaith Level: {r.CurrentFaithLevel}/4\nQuest: {r.QuestDescription}";
        List<string> options = new List<string>();

        if (r.CurrentQuestStatus == QuestStatus.ReadyToTurnIn)
        {
            options.Add("Complete Quest & Claim Reward");
        }
        else if (r.CurrentQuestStatus == QuestStatus.GatheringItem)
        {
            InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
            if (inv != null && inv.HasItem(r.RequiredItemID))
            {
                options.Add($"Sacrifice required item to the Altar");
            }
        }
        
        options.Add("Leave");
        new Prompt(msg, options, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (_rm.CurrentReligion == null)
        {
            // Joining logic
            if (decisionIndex < _offeredReligions.Count)
            {
                Religion chosenReligion = _offeredReligions[decisionIndex];
                _rm.JoinReligion(chosenReligion);
            }
            else
            {
                TextOutputter.Instance.OutputText("You decline the offer of faith and move on.");
            }
            _offeredReligions = new List<Religion>();
        }
        else
        {
            // Member interaction logic
            Religion r = _rm.CurrentReligion;
            List<string> options = new List<string>();
            bool canTurnIn = r.CurrentQuestStatus == QuestStatus.ReadyToTurnIn;
            bool canSacrifice = false;
            
            InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
            if (r.CurrentQuestStatus == QuestStatus.GatheringItem && inv != null && inv.HasItem(r.RequiredItemID))
            {
                canSacrifice = true;
            }

            if (canTurnIn) options.Add("complete");
            else if (canSacrifice) options.Add("sacrifice");
            options.Add("leave");

            string choice = options[decisionIndex];
            if (choice == "complete")
            {
                r.CompleteQuest();
            }
            else if (choice == "sacrifice")
            {
                inv.RemoveItemByID(r.RequiredItemID);
                TextOutputter.Instance.OutputText($"You sacrifice the required item to the altar!");
                r.SetQuestReady();
                TextOutputter.Instance.OutputText("The celestial forces are pleased. Your quest is ready for completion.");
            }
            else
            {
                TextOutputter.Instance.OutputText("You leave the holy site.");
            }
        }

        ResolveEncounter();
    }
}
