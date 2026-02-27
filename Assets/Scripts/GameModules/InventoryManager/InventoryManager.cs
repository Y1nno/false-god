using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : GameModule
{
    private EquipmentManager _eq = new EquipmentManager();
    private Inventory _inv = new Inventory();

    #region Public API

    #region Equipment Methods

    public override void AttachDefaultObservers()
    {
        // none for now
    }

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
    public void RemoveItemFromInventory(int itemID)
    {
        for (int i = 0; i < _inv.GetAllItems().Count; i++)
        {
            if (_inv.GetAllItems()[i].ID == itemID)
            {
                _inv.RemoveItem(_inv.GetAllItems()[i]);
                break;
            }
        }
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

    public int GetItemCount(int itemID)
    {
        return _inv.GetItemCount(itemID);
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

    #region Equipment Methods
    public int CalculateSecondaryStatFromEquipment(SecondaryStat secondaryStat)
    {
        int total = 0;
        foreach (Equipment equipment in _eq.GetAllEquippedItems().Values)
        {
            switch (secondaryStat)
                {
                    case SecondaryStat.SPATK:
                        total += equipment.SpecialAttack;
                        break;
                    case SecondaryStat.SPDEF:
                        total += equipment.SpecialDefense;
                        break;
                    case SecondaryStat.CRIT:
                        total += equipment.CritChance;
                        break;
                    case SecondaryStat.EVDE:
                        total += equipment.DodgeChance;
                        break;
                }
        }
        return total;
    }
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
