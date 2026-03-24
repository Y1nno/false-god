using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//TODO: Add inventory size limit and logic for refusing requests to add to inventory. 

public class InventoryManager : GameModule
{
    private EquipmentManager _eq = new EquipmentManager();
    private Inventory _inv = new Inventory();

    #region Public API

    public List<Item> UnEquippedItems { get; private set; } = new List<Item>();

    public override void AttachDefaultObservers()
    {
        // Automatically find and attach UI components that need to refresh on inventory changes
        MaterialsContainer mc = GameObject.FindAnyObjectByType<MaterialsContainer>();
        if (mc != null) AttachObserver(mc);

        InputInterface ii = GameObject.FindAnyObjectByType<InputInterface>();
        if (ii != null) AttachObserver(ii);

        SpellbookUI sui = GameObject.FindAnyObjectByType<SpellbookUI>();
        if (sui != null) AttachObserver(sui);
    }

    public bool HasRelic(string relicName)
    {
        // Check unequipped items
        bool inInventory = UnEquippedItems.Any(i => i is RelicInstance r && (r.GetName() == relicName || r.BaseData.ItemName == relicName));
        if (inInventory) return true;

        // Check active relics
        RelicManager rm = RunManager.Instance.GetService<RelicManager>();
        return rm != null && rm.HasRelic(relicName);
    }

    #region Inventory Logic

    public void AddItemToInventory(Item item)
    {
        if (item == null) return;

        // Force Relic Uniqueness: Re-check here to catch manual additions (test buttons, etc)
        if (item is RelicInstance relic && HasRelic(relic.GetName()))
        {
            TextOutputter.Instance.OutputText($"You already possess '{relic.GetName()}'. Duplicate relic rejected.");
            return;
        }

        // Consolidation Logic: Merge MaterialInstances if they are identical
        if (item is MaterialInstance newMat)
        {
            MaterialInstance existing = UnEquippedItems.Find(i => i is MaterialInstance m && m.BaseData == newMat.BaseData) as MaterialInstance;
            if (existing != null)
            {
                existing.Quantity += newMat.Quantity;
                TextOutputter.Instance.OutputText($"Added {newMat.GetName()} x{newMat.Quantity} to Inventory (Total: {existing.Quantity}).");
                Notify(EventType.ItemAcquired);
                return;
            }
        }

        UnEquippedItems.Add(item);
        
        string itemName = item.GetName();
                          
        TextOutputter.Instance.OutputText($"Added {itemName} to Inventory.");
        Notify(EventType.ItemAcquired);
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (UnEquippedItems.Contains(item))
        {
            UnEquippedItems.Remove(item);
            Notify(EventType.ItemRemoved);
        }
    }

    // Handles the UI passing down requests to either wear a weapon or drink a potion
    public void UseOrEquipItem(Item itemToHandle)
    {
        if (itemToHandle == null || !UnEquippedItems.Contains(itemToHandle)) return;

        if (itemToHandle is Equipment equipment)
        {
            // Pass the runtime Equipment instance directly to preserve its state (durability, modifiers)
            RunManager.Instance.GetService<EquipmentManager>()?.EquipItem(equipment);
            RemoveItemFromInventory(equipment);
        }
        else if (itemToHandle is ConsumableInstance consumableInstance)
        {
            RemoveItemFromInventory(consumableInstance); // Drink it
            
            CombatManager cm = RunManager.Instance.GetService<CombatManager>();
            if (cm != null && cm.Pcm != null)
            {
                consumableInstance.Use(cm.Pcm); 
            }
        }
        else if (itemToHandle is KeyInstance key)
        {
            TextOutputter.Instance.OutputText("Keys must be used directly on locked chests.");
        }
    }

    #endregion
    #endregion
}

public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Hands,
    Belt,
    Weapon,
    OffHand,
    Accessory1,
    Accessory2
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
