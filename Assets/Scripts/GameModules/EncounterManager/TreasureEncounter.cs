using UnityEngine;
using System.Collections.Generic;

public class TreasureEncounter : Encounter, IObserver
{
    private bool _isLocked = true;
    private List<KeyInstance> _availableKeys = new List<KeyInstance>();
    private bool _waitingForForceRoll = false;

    public TreasureEncounter(float difficulty) : base(difficulty)
    {
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        DiceRoller.Instance.AttachObserver(this);
        ShowLockPrompt();
    }

    private void ShowLockPrompt()
    {
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        _availableKeys.Clear();

        List<string> options = new List<string>();
        
        // Find keys in inventory
        foreach (var item in invm.UnEquippedItems)
        {
            if (item is KeyInstance key)
            {
                _availableKeys.Add(key);
                options.Add($"Use {key.BaseData.ItemName} ({key.RemainingUses} uses)");
            }
        }

        options.Add("Force Open (STR Stat Check)");
        options.Add("Leave");

        string promptText = "The chest is locked. How would you like to proceed?";
        new Prompt(promptText, options, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (!_isLocked)
        {
            ResolveEncounter();
            return;
        }

        // Decision logic
        if (decisionIndex < _availableKeys.Count)
        {
            // Using a key
            KeyInstance selectedKey = _availableKeys[decisionIndex];
            if (selectedKey.TryUse())
            {
                TextOutputter.Instance.OutputText($"Used {selectedKey.BaseData.ItemName}. The chest clicks open!");
                _isLocked = false;
            }
            else
            {
                TextOutputter.Instance.OutputText($"Used {selectedKey.BaseData.ItemName}... but it failed to open the chest.");
            }

            // Clean up destroyed keys
            if (selectedKey.RemainingUses <= 0)
            {
                RunManager.Instance.GetService<InventoryManager>().RemoveItemFromInventory(selectedKey);
                TextOutputter.Instance.OutputText($"{selectedKey.BaseData.ItemName} was destroyed.");
            }

            if (!_isLocked) ResolveEncounter();
            else ShowLockPrompt(); // Re-prompt if still locked (failed lockpick)
        }
        else if (decisionIndex == _availableKeys.Count)
        {
            // Force Open (STR check)
            int threshold = 10 + (int)(_difficulty * 0.5f);
            
            TextOutputter.Instance.OutputText($"Attempting to force the chest open...");
            
            _waitingForForceRoll = true;
            DiceRoller.Instance.RollForStat(Stat.STR, threshold);
        }
        else
        {
            // Leave
            ResolveEncounter();
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (_waitingForForceRoll && eventType == EventType.DiceRollFinalized)
        {
            _waitingForForceRoll = false;
            HandleForceRollResult();
        }
    }

    private void HandleForceRollResult()
    {
        DiceRollResult result = DiceRoller.Instance.LastRollResult;
        
        if (result.IsSuccess)
        {
            TextOutputter.Instance.OutputText("With a mighty heave, you smash the lock! The chest is open.");
            _isLocked = false;
            ResolveEncounter();
        }
        else
        {
            TextOutputter.Instance.OutputText("The chest won't budge. Your strength was not enough.");
            ShowLockPrompt();
        }
    }

    public override void ResolveEncounter()
    {
        DiceRoller.Instance.DetachObserver(this);
        base.ResolveEncounter();
    }
}
