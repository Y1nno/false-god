using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : GameModule
{
    // The player's overall inventory of unequipped items
    public List<EquipmentSO> Inventory { get; private set; } = new List<EquipmentSO>();

    // What the player is currently wearing
    public Dictionary<EquipmentSlot, EquipmentSO> EquippedItems { get; private set; } = new Dictionary<EquipmentSlot, EquipmentSO>();

    public override void AttachDefaultObservers()
    {
        // Add default observers here if needed later (e.g., listening for combat start)
    }

    public void AddEquipmentToInventory(EquipmentSO equipment)
    {
        if (equipment == null) return;
        Inventory.Add(equipment);
        TextOutputter.Instance.OutputText($"Added {equipment.ItemName} to Inventory.");
    }

    public void RemoveEquipmentFromInventory(EquipmentSO equipment)
    {
        if (Inventory.Contains(equipment))
        {
            Inventory.Remove(equipment);
        }
    }

    public void EquipItem(EquipmentSO newItem)
    {
        if (newItem == null) return;

        // If the player isn't holding this item in their inventory, they can't equip it!
        if (!Inventory.Contains(newItem))
        {
            Debug.LogWarning($"Tried to equip {newItem.ItemName}, but it's not in the inventory!");
            return;
        }

        // 1. Check if something is already in this slot
        if (EquippedItems.TryGetValue(newItem.Slot, out EquipmentSO currentlyEquipped))
        {
            // 2. If yes, unequip it and put it BACK into the inventory
            UnequipItem(newItem.Slot);
        }

        // 3. Equip the new item
        EquippedItems[newItem.Slot] = newItem;
        
        // 4. Remove the newly equipped item from the general inventory pool
        RemoveEquipmentFromInventory(newItem);

        TextOutputter.Instance.OutputText($"Equipped {newItem.ItemName} to {newItem.Slot}.");

        // 5. Tell the PlayerManager to recalculate stats (Optional event could go here)
        // RunManager.Instance.GetService<PlayerManager>().PlayerStats.RecalculateStats(); 
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (EquippedItems.TryGetValue(slot, out EquipmentSO itemToUnequip))
        {
            EquippedItems.Remove(slot);
            AddEquipmentToInventory(itemToUnequip); // Put it back in our pockets
            TextOutputter.Instance.OutputText($"Unequipped {itemToUnequip.ItemName}.");
        }
    }

    public bool HasMoveFirstAvailable(out EquipmentSO moveFirstItem, out int traitIndex)
    {
        moveFirstItem = null;
        traitIndex = -1;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                for (int i = 0; i < item.Traits.Count; i++)
                {
                    if (item.Traits[i].Trait == EquipmentTrait.MoveFirst)
                    {
                        moveFirstItem = item;
                        traitIndex = i;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void TriggerMoveFirstCooldown()
    {
        if (HasMoveFirstAvailable(out EquipmentSO item, out int traitIndex))
        {
            var trait = item.Traits[traitIndex];
            
            // If the cooldown counts in Combat Rounds, we must add +1 to the duration 
            // since the cooldown will instantly tick down at the end of THIS round.
            if (trait.CooldownType == CooldownType.CombatRounds) 
            {
                trait.CurrentCooldown = trait.CooldownDuration + 1;
            } 
            else 
            {
                trait.CurrentCooldown = trait.CooldownDuration;
            }
            
            item.Traits[traitIndex] = trait; // Re-assign struct
        }
    }

    public void TickCooldowns(CooldownType type)
    {
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                for (int i = 0; i < item.Traits.Count; i++)
                {
                    var trait = item.Traits[i];
                    if (trait.Trait == EquipmentTrait.MoveFirst && trait.CooldownType == type && trait.CurrentCooldown > 0)
                    {
                        trait.CurrentCooldown--;
                        item.Traits[i] = trait; // Re-assign struct
                    }
                }
            }
        }
    }

    // --- Stat Calculation Helpers ---
    
    public int GetTotalBonus(System.Func<EquipmentSO, int> statSelector)
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            total += statSelector(item);
        }
        return total;
    }

    public int GetTotalPhysicalAttack() => GetTotalBonus(item => item.PhysicalAttack.value);
    public int GetTotalSpecialAttack() => GetTotalBonus(item => item.SpecialAttack.value);
    public int GetTotalPhysicalDefense() => GetTotalBonus(item => item.PhysicalDefense.value);
    public int GetTotalSpecialDefense() => GetTotalBonus(item => item.SpecialDefense.value);
    public int GetTotalSTR() => GetTotalBonus(item => item.STR.value);
    public int GetTotalDEX() => GetTotalBonus(item => item.DEX.value);
    public int GetTotalINT() => GetTotalBonus(item => item.INT.value);
    public int GetTotalSPD() => GetTotalBonus(item => item.SPD.value);
}
