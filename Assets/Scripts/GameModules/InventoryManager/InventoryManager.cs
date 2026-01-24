using UnityEngine;
using System.Collections.Generic;

public class InventoryManager
{
    private Equipment _eq = new Equipment();
    private Inventory _inv = new Inventory();
    
    #region Public API

    #region Equipment Methods
    public void EquipItem(EquipmentSlot slot, Item item)
    {
        if (item.CanBeEquipped() == false)
        {
            // Item cannot be equipped
            return;
        }
        
        Item oldItem = _eq.EquipItem(slot, item);
        if (oldItem != null)
        {
            _inv.AddItem(oldItem);
        }
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return _eq.GetEquippedItem(slot);
    }

    public Dictionary<EquipmentSlot, Item> GetAllEquippedItems()
    {
        return _eq.GetAllEquippedItems();
    }

    public bool IsSlotOccupied(EquipmentSlot slot)
    {
        return _eq.IsSlotOccupied(slot);
    }

    public void UnequipItem(EquipmentSlot slot)
    {      
        if (_eq.GetEquippedItem(slot)?.CanBeUnequipped() == false)
        {
            // Item cannot be unequipped
            return;
        }
        Item oldItem = _eq.UnequipItem(slot);
        if (oldItem != null)
        {
            _inv.AddItem(oldItem);
        }
    }

    public void UnequipItem(EquipmentSlot slot, int position)
    {
        Item item = _eq.GetEquippedItem(slot);
        if (item == null)
        {
            // No item equipped in the specified slot
            return;
        }
        if (item.CanBeUnequipped() == false)
        {
            // Item cannot be unequipped
            return;
        }
        UnequipItem(slot);
        _inv.AddItem(item, position);
    }


    #endregion

    #region Inventory Methods
    public void AddItemToInventory(Item item)
    {
        _inv.AddItem(item);
    }

    public void RemoveItemFromInventory(Item item)
    {
        _inv.RemoveItem(item);
    }

    public void DiscardItemFromInventory(Item item)
    {
        _inv.DiscardItem(item);
    }

    public List<Item> GetAllInventoryItems()
    {
        return _inv.GetAllItems();
    }

    public bool IsItemInInventory(Item item)
    {
        return _inv.ContainsItem(item);
    }

    public void UseItem(Item item)
    {
        if (_inv.ContainsItem(item))
        {
            item.Use();
            _inv.RemoveItem(item);
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
