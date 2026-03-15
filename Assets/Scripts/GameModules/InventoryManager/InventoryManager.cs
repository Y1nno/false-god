using UnityEngine;
using System.Collections.Generic;

//TODO: Add inventory size limit and logic for refusing requests to add to inventory. 

public class InventoryManager : GameModule
{
    private EquipmentManager _eq = new EquipmentManager();
    private Inventory _inv = new Inventory();

    #region Public API

    public List<Item> UnEquippedItems { get; private set; } = new List<Item>();

    #region Inventory Logic

    public void AddItemToInventory(Item item)
    {
        if (item == null) return;

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
            // Equipment wrapper needs to unwrap and fetch its underlying SO if the Equipper requires it
            if (equipment.BaseData != null)
            {
                RunManager.Instance.GetService<EquipmentManager>()?.EquipItem(equipment.BaseData);
                RemoveItemFromInventory(equipment); // Successfully passed to EquipmentManager, so remove from unequipped pool
            }
        }
        else if (itemToHandle is ConsumableInstance consumableInstance)
        {
            RemoveItemFromInventory(consumableInstance); // Drink it
            
            CombatManager cm = RunManager.Instance.GetService<CombatManager>();
            if (cm != null && cm.CurrentBattle != null && cm.CurrentBattle.Pcm != null)
            {
                consumableInstance.Use(cm.CurrentBattle.Pcm); 
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
