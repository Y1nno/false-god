using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FountainEncounter : Encounter
{
    private Item _emptyBottle = null;

    public FountainEncounter(float difficulty) : base(difficulty) { }

    public override void StartEncounter()
    {
        base.StartEncounter();
        
        InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
        _emptyBottle = inv.UnEquippedItems.Find(i => i.GetName() == "Empty Bottle");

        List<string> options = new List<string>();
        if (_emptyBottle != null)
        {
            options.Add("Fill Empty Bottle");
        }
        options.Add("Heal (35% HP/Mana)");
        options.Add("Leave");

        new Prompt("A crystal-clear fountain bubbles nearby. What do you do?", options, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        Combatant player = cm.Pcm;

        // Map decision index back to action based on whether bottle option was available
        string choice = "";
        if (_emptyBottle != null)
        {
            if (decisionIndex == 0) choice = "fill";
            else if (decisionIndex == 1) choice = "heal";
            else choice = "leave";
        }
        else
        {
            if (decisionIndex == 0) choice = "heal";
            else choice = "leave";
        }

        if (choice == "fill")
        {
            inv.RemoveItemFromInventory(_emptyBottle);
            Item fullBottle = ItemFactory.CreateItemByID("158");
            if (fullBottle != null)
            {
                inv.AddItemToInventory(fullBottle);
                TextOutputter.Instance.OutputText("You fill your empty bottle with the fountain's water. It's now a Full Bottle!");
            }
            else
            {
                Debug.LogError("FountainEncounter: Failed to create Full Bottle item from ID 158.");
            }
        }
        else if (choice == "heal")
        {
            if (player != null)
            {
                int hpHeal = Mathf.RoundToInt(player.GetHealth().MaxValue * 0.35f);
                int mpHeal = Mathf.RoundToInt(player.GetMana().MaxValue * 0.35f);
                player.Heal(hpHeal);
                player.RestoreMana(mpHeal);
                TextOutputter.Instance.OutputText($"You drink deeply from the fountain, restoring {hpHeal} HP and {mpHeal} Mana.");
            }
        }
        else
        {
            TextOutputter.Instance.OutputText("You leave the fountain behind.");
        }
        ResolveEncounter();
    }
}
