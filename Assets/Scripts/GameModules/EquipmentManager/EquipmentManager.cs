using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : GameModule
{
    // The player's overall inventory of unequipped items
    public List<EquipmentSO> Inventory { get; private set; } = new List<EquipmentSO>();

    // What the player is currently wearing
    public Dictionary<EquipmentSlot, EquipmentSO> EquippedItems { get; private set; } = new Dictionary<EquipmentSlot, EquipmentSO>();
    
    // Internal flag to prevent cooldown from ticking down on the very round it was used
    private bool _moveFirstUsedThisRound = false;

    // Composite Subject Implementation due to GameModule inheritance
    private List<IObserver> _observers = new List<IObserver>();

    public void AddObserver(IObserver observer)
    {
        if (!_observers.Contains(observer)) _observers.Add(observer);
    }
    public void RemoveObserver(IObserver observer)
    {
        if (_observers.Contains(observer)) _observers.Remove(observer);
    }
    public new void Notify(EventType eventType)
    {
        List<IObserver> observersCopy = new List<IObserver>(_observers);
        foreach (var observer in observersCopy)
        {
            observer.OnNotify(this, eventType);
        }
        
        // Push notification back up to base GameModule (Subject) in case other listeners attached externally
        base.Notify(eventType);
    }

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

        // 2.5 Two-Handed Weapon / OffHand conflict resolution
        if (newItem.IsTwoHanded)
        {
            if (EquippedItems.ContainsKey(EquipmentSlot.OffHand))
            {
                UnequipItem(EquipmentSlot.OffHand);
            }
        }
        else if (newItem.Slot == EquipmentSlot.OffHand)
        {
            if (EquippedItems.TryGetValue(EquipmentSlot.Weapon, out EquipmentSO currentWeapon))
            {
                if (currentWeapon.IsTwoHanded)
                {
                    UnequipItem(EquipmentSlot.Weapon);
                }
            }
        }

        // 3. Equip the new item
        EquippedItems[newItem.Slot] = newItem;
        
        // 4. Remove the newly equipped item from the general inventory pool
        RemoveEquipmentFromInventory(newItem);

        TextOutputter.Instance.OutputText($"Equipped {newItem.ItemName} to {newItem.Slot}.");

        // 5. Tell the PlayerManager to recalculate stats (Optional event could go here)
        // 5. Tell the PlayerManager to recalculate stats (Optional event could go here)
        RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
        Notify(EventType.EquipmentChanged);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (EquippedItems.TryGetValue(slot, out EquipmentSO itemToUnequip))
        {
            EquippedItems.Remove(slot);
            AddEquipmentToInventory(itemToUnequip); // Put it back in our pockets
            TextOutputter.Instance.OutputText($"Unequipped {itemToUnequip.ItemName}.");
            RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
            Notify(EventType.EquipmentChanged);
        }
    }

    public bool IsHoldingTwoHandedWeapon()
    {
        if (EquippedItems.TryGetValue(EquipmentSlot.Weapon, out EquipmentSO currentWeapon))
        {
            return currentWeapon.IsTwoHanded;
        }
        return false;
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

    public bool HasTrait(EquipmentTrait searchTrait)
    {
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == searchTrait) return true;
                }
            }
        }
        return false;
    }

    public bool HasTraitAvailable(EquipmentTrait searchTrait, out EquipmentSO foundItem, out int traitIndex)
    {
        foundItem = null;
        traitIndex = -1;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                for (int i = 0; i < item.Traits.Count; i++)
                {
                    if (item.Traits[i].Trait == searchTrait)
                    {
                        foundItem = item;
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
            trait.CurrentCooldown = trait.CooldownDuration;
            item.Traits[traitIndex] = trait; // Re-assign struct
            _moveFirstUsedThisRound = true;
        }
    }

    public void TickCooldowns(CooldownType type)
    {
        if (type == CooldownType.CombatRounds && _moveFirstUsedThisRound)
        {
            _moveFirstUsedThisRound = false;
            return; // Skip ticking down the round we just used the ability
        }

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
    public int GetTotalAllStats()
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.AllStats)
                    {
                        total += trait.Value;
                    }
                }
            }
        }
        return total;
    }

    public int GetTotalSTR() => GetTotalBonus(item => item.STR.value) + GetTotalAllStats();
    public int GetTotalDEX() => GetTotalBonus(item => item.DEX.value) + GetTotalAllStats();
    public int GetTotalINT() => GetTotalBonus(item => item.INT.value) + GetTotalAllStats();
    public int GetTotalSPD() => GetTotalBonus(item => item.SPD.value) + GetTotalAllStats();
    public int GetTotalLCK() => GetTotalAllStats(); // LCK is exclusively handled by AllStats right now
    public int GetTotalBlockChance() => GetTotalBonus(item => item.BlockChance.value);
    public int GetTotalBlockAmount() => GetTotalBonus(item => item.BlockAmount.value);
    
    public int GetTotalSpellReflectChance()
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.SpellReflect)
                    {
                        total += trait.Value;
                    }
                }
            }
        }
        return total;
    }

    public int GetTotalMaxHPPercentage()
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.MaxHP)
                    {
                        total += trait.Value;
                    }
                }
            }
        }
        return total;
    }
}
