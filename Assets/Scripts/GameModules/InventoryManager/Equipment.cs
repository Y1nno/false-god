using UnityEngine;
using System.Collections.Generic;

public class Equipment
{
    public Dictionary<EquipmentSlot, Item> _equipment = new Dictionary<EquipmentSlot, Item>();

    public Item EquipItem(EquipmentSlot slot, Item item)
    {
        if (item == null) return null;

        Item oldItem = GetEquippedItem(slot);

        // If equipping a 2H weapon, OffHand must be removable or empty
        if (slot == EquipmentSlot.Weapon && item.IsTwoHanded())
        {
            Item currentOffHand = GetEquippedItem(EquipmentSlot.OffHand);
            if (currentOffHand != null && !currentOffHand.CanBeUnequipped())
                return null;

            if (currentOffHand != null)
                UnequipItem(EquipmentSlot.OffHand);
        }

        // If equipping OffHand, a currently equipped 2H weapon must be removable (or absent)
        if (slot == EquipmentSlot.OffHand)
        {
            Item currentWeapon = GetEquippedItem(EquipmentSlot.Weapon);
            if (currentWeapon != null && currentWeapon.IsTwoHanded())
            {
                if (!currentWeapon.CanBeUnequipped())
                    return null;

                UnequipItem(EquipmentSlot.Weapon);
            }
        }
        if (oldItem != null && !oldItem.CanBeUnequipped())
            return null;

        _equipment[slot] = item;
        return oldItem;
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        if (_equipment.ContainsKey(slot))
        {
            return _equipment[slot];
        }
        return null;
    }

    public Dictionary<EquipmentSlot, Item> GetAllEquippedItems()
    {
        return _equipment;
    }

    public bool IsSlotOccupied(EquipmentSlot slot)
    {
        return _equipment.ContainsKey(slot) && _equipment[slot] != null;
    }

    public Item UnequipItem(EquipmentSlot slot)
    {
        // Ensure item can be unequipped
        if (_equipment.ContainsKey(slot) && _equipment[slot] != null && _equipment[slot].CanBeUnequipped())
        {
            Item item = _equipment[slot];
            _equipment[slot] = null;
            return item;
        }
        return null;
    }
}