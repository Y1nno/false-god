using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BlacksmithEncounter : Encounter
{
    private enum BlacksmithState { Main, Smithing }
    private BlacksmithState _state = BlacksmithState.Main;
    private List<Equipment> _upgradeableItems = new List<Equipment>();

    public BlacksmithEncounter(float difficulty) : base(difficulty) { }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("The heat of the forge hits you. 'Bring me your steel,' the blacksmith grunts.");
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _state = BlacksmithState.Main;
        List<string> choices = new List<string> { "Smith", "Leave" };
        new Prompt("Blacksmith Options:", choices, this);
    }

    private void ShowSmithMenu()
    {
        _state = BlacksmithState.Smithing;
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        _upgradeableItems = eqm.EquippedItems.Values.Where(item => item.UpgradeLevel < 10 && item.GetUpgradeCost() > 0).ToList();

        if (_upgradeableItems.Count == 0)
        {
            TextOutputter.Instance.OutputText("'You have no equipment to smith.'");
            ShowMainMenu();
            return;
        }

        List<string> choices = new List<string>();
        foreach (var item in _upgradeableItems)
        {
            int cost = item.GetUpgradeCost();
            string req = item.UpgradeLevel >= 5 ? " + Gemshard" : "";
            choices.Add($"{item.GetName()} -> {item.UpgradeLevel + 1} ({cost}g{req})");
        }
        choices.Add("Refresh List");
        choices.Add("Back");

        new Prompt("Select an item to upgrade:", choices, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (_state == BlacksmithState.Main)
        {
            if (decisionIndex == 0) ShowSmithMenu();
            else ResolveEncounter();
        }
        else if (_state == BlacksmithState.Smithing)
        {
            if (decisionIndex < _upgradeableItems.Count)
            {
                UpgradeItem(_upgradeableItems[decisionIndex]);
            }
            else if (decisionIndex == _upgradeableItems.Count)
            {
                ShowSmithMenu(); // Refresh
            }
            else
            {
                ShowMainMenu();
            }
        }
    }

    private void UpgradeItem(Equipment item)
    {
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
        int cost = item.GetUpgradeCost();

        if (em == null || !em.CanSpendGold(cost))
        {
            TextOutputter.Instance.OutputText("'You don't have enough gold for this upgrade.'");
            ShowSmithMenu();
            return;
        }

        // Check for Gemshard if level >= 5
        MaterialInstance gemshard = null;
        if (item.UpgradeLevel >= 5)
        {
            gemshard = inv?.UnEquippedItems.Find(i => i is MaterialInstance m && m.GetName() == "Gem Shard") as MaterialInstance;
            if (gemshard == null)
            {
                TextOutputter.Instance.OutputText("'You need a Gemshard to upgrade this item further.'");
                ShowSmithMenu();
                return;
            }
        }

        // Deduct resources
        em.SpendGold(cost);
        if (gemshard != null)
        {
            if (gemshard.Quantity > 1) gemshard.Quantity--;
            else inv.RemoveItemFromInventory(gemshard);
        }

        // Upgrade
        item.UpgradeLevel++;
        TextOutputter.Instance.OutputText($"Clang! Your {item.BaseItemName} is now +{item.UpgradeLevel}.");
        
        // Refresh stats and UI
        eqm.RefreshEquipment();
        
        ShowSmithMenu(); // Loop back
    }
}
