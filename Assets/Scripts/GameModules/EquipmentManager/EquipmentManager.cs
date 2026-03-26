using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : GameModule
{
    // What the player is currently wearing
    public Dictionary<EquipmentSlot, Equipment> EquippedItems { get; private set; } = new Dictionary<EquipmentSlot, Equipment>();
    
    // Internal flag to prevent cooldown from ticking down on the very round it was used
    private bool _moveFirstUsedThisRound = false;

    public Equipment GetEquippedItem(EquipmentSlot slot)
    {
        EquippedItems.TryGetValue(slot, out Equipment item);
        return item;
    }

    public void RestoreState(List<SerializableItem> items)
    {
        EquippedItems.Clear();
        foreach (var sItem in items)
        {
            if (sItem.ItemType != "Equipment") continue;

            Equipment eq = ReconstructEquipment(sItem);
            if (eq != null)
            {
                EquippedItems[sItem.EquippedSlot] = eq;
            }
        }
        RefreshEquipment();
    }

    public Equipment ReconstructEquipment(SerializableItem sEq)
    {
        // Search Resources for EquipmentSO with matching ItemID in Items subfolder
        EquipmentSO[] allSOs = Resources.LoadAll<EquipmentSO>("Items");
        EquipmentSO baseData = System.Array.Find(allSOs, so => so.ItemID == sEq.ItemID);
        
        if (baseData == null)
        {
            Debug.LogError($"SaveManager: Could not find EquipmentSO with ID {sEq.ItemID} to reconstruct item.");
            return null;
        }

        Equipment eq = new Equipment(baseData);
        eq.UpgradeLevel = sEq.UpgradeLevel;
        eq.DegradeEquipment(eq.MaxDurability - sEq.Durability); // Set durability
        eq.Prefixes.Clear();
        eq.Prefixes.AddRange(sEq.Prefixes);
        eq.Suffixes.Clear();
        eq.Suffixes.AddRange(sEq.Suffixes);
        
        eq._basePhysicalDefense = sEq.BasePhysDef;
        eq._baseSpecialDefense = sEq.BaseSpecDef;
        eq._basePhysicalAttack = sEq.BasePhysAtk;
        eq._baseSpecialAttack = sEq.BaseSpecAtk;
        eq._baseSTR = sEq.BaseSTR;
        eq._baseDEX = sEq.BaseDEX;
        eq._baseINT = sEq.BaseINT;
        eq._baseSPD = sEq.BaseSPD;
        eq._baseCritChance = sEq.BaseCrit;
        eq._baseBlockChance = sEq.BaseBlock;
        eq._baseBlockAmount = sEq.BaseBlockAmt;
        eq._baseDodgeChance = sEq.BaseDodge;

        // Rebuild Traits list from modifiers if they were trait-based
        foreach (var mod in eq.Prefixes) {
            if (mod.EffectType == ModifierEffectType.Trait) eq.Traits.Add(new TraitWithValue(mod.Trait, mod.Value) { CooldownType = mod.CooldownType, CooldownDuration = mod.CooldownDuration });
        }
        foreach (var mod in eq.Suffixes) {
            if (mod.EffectType == ModifierEffectType.Trait) eq.Traits.Add(new TraitWithValue(mod.Trait, mod.Value) { CooldownType = mod.CooldownType, CooldownDuration = mod.CooldownDuration });
        }

        return eq;
    }

    public override void AttachDefaultObservers()
    {
        // Add default observers here if needed later (e.g., listening for combat start)
    }

    public void EquipItem(Equipment newItem)
    {
        if (newItem == null) return;
        EquipItemToSlot(newItem, newItem.Slot);
    }

    public void EquipItemToSlot(Equipment newItem, EquipmentSlot targetSlot)
    {
        if (newItem == null) return;

        // 1. Check if something is already in this slot
        if (EquippedItems.TryGetValue(targetSlot, out Equipment currentlyEquipped))
        {
            // 2. If yes, unequip it and put it BACK into the InventoryManager's pool
            UnequipItem(targetSlot);
        }

        // 2.5 Two-Handed Weapon / OffHand conflict resolution
        if (newItem.IsTwoHanded && targetSlot == EquipmentSlot.Weapon)
        {
            if (EquippedItems.ContainsKey(EquipmentSlot.OffHand))
            {
                UnequipItem(EquipmentSlot.OffHand);
            }
        }
        else if (targetSlot == EquipmentSlot.OffHand)
        {
            if (EquippedItems.TryGetValue(EquipmentSlot.Weapon, out Equipment currentWeapon))
            {
                if (currentWeapon.IsTwoHanded)
                {
                    UnequipItem(EquipmentSlot.Weapon);
                }
            }
        }

        // 3. Equip the new item to the TARGET slot, overriding its internal Slot property if needed
        EquippedItems[targetSlot] = newItem;

        TextOutputter.Instance.OutputText($"Equipped {newItem.GetName()} to {targetSlot}.");

        // 5. Tell the PlayerManager to recalculate stats
        RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
        Notify(EventType.EquipmentChanged);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (EquippedItems.TryGetValue(slot, out Equipment itemToUnequip))
        {
            EquippedItems.Remove(slot);

            // Restore the original wrapper to inventory
            RunManager.Instance.GetService<InventoryManager>()?.AddItemToInventory(itemToUnequip);

            TextOutputter.Instance.OutputText($"Unequipped {itemToUnequip.GetName()}.");
            RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
            Notify(EventType.EquipmentChanged);
        }
    }

    public void RefreshEquipment()
    {
        RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
        Notify(EventType.EquipmentChanged);
    }

    public bool IsHoldingTwoHandedWeapon()
    {
        if (EquippedItems.TryGetValue(EquipmentSlot.Weapon, out Equipment currentWeapon))
        {
            return currentWeapon.IsTwoHanded;
        }
        return false;
    }

    public bool HasMoveFirstAvailable(out Equipment moveFirstItem, out int traitIndex)
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

    public bool HasTraitAvailable(EquipmentTrait searchTrait, out Equipment foundItem, out int traitIndex)
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
        if (HasMoveFirstAvailable(out Equipment item, out int traitIndex))
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
    
    public int GetTotalBonus(System.Func<Equipment, int> statSelector)
    {
        float total = 0;
        foreach (var item in EquippedItems.Values)
        {
            total += statSelector(item) * item.GetDurabilityMultiplier();
        }
        return Mathf.RoundToInt(total);
    }

    public int GetTotalPhysicalAttack() => GetTotalBonus(item => item.PhysicalAttack);
    public int GetTotalSpecialAttack() => GetTotalBonus(item => item.SpecialAttack);
    public int GetTotalPhysicalDefense() => GetTotalBonus(item => item.PhysicalDefense);
    public int GetTotalSpecialDefense() => GetTotalBonus(item => item.SpecialDefense);
    public int GetTotalAllStats()
    {
        float total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.AllStats)
                    {
                        total += trait.Value * item.GetDurabilityMultiplier();
                    }
                }
            }
        }
        return Mathf.RoundToInt(total);
    }

    public int GetTotalSTR() => GetTotalBonus(item => item.STR) + GetTotalAllStats();
    public int GetTotalDEX() => GetTotalBonus(item => item.DEX) + GetTotalAllStats();
    public int GetTotalINT() => GetTotalBonus(item => item.INT) + GetTotalAllStats();
    public int GetTotalSPD() => GetTotalBonus(item => item.SPD) + GetTotalAllStats();
    // public int GetTotalLCK() => GetTotalAllStats(); // LCK is removed
    public int GetTotalBlockChance() => GetTotalBonus(item => item.BlockChance);
    public int GetTotalBlockAmount() => GetTotalBonus(item => item.BlockAmount);
    
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

    public int GetTotalSplashDamagePercentage()
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.SplashDamage)
                    {
                        total += trait.Value;
                    }
                }
            }
        }
        return total;
    }

    public int GetTotalBonusBlock()
    {
        int total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.BonusBlock)
                    {
                        total += trait.Value;
                    }
                }
            }
        }
        return total;
    }

    public float GetTotalShieldingMultiplier()
    {
        float total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.Shielding)
                    {
                        total += trait.Value / 100.0f;
                    }
                }
            }
        }
        return 1.0f + total;
    }

    public int GetBaseBlockAmount(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 2,
            Rarity.Uncommon => 4,
            Rarity.Rare => 6,
            Rarity.Epic => 10,
            Rarity.Legendary => 10,
            _ => 0
        };
    }

    /*
    public int GetTotalMagicFind()
    {
        float total = 0;
        foreach (var item in EquippedItems.Values)
        {
            if (item.Traits != null)
            {
                foreach (var trait in item.Traits)
                {
                    if (trait.Trait == EquipmentTrait.MagicFind)
                    {
                        total += trait.Value * item.GetDurabilityMultiplier();
                    }
                }
            }
        }
        return Mathf.RoundToInt(total);
    }
    */

    // --- Durability Degradation ---
    
    private bool RollDurabilityDegradeChance(Equipment item)
    {
        // Forge-Tongue Brotherhood Lvl 4: Forge Blessing (No weapon durability loss)
        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (rm?.CurrentReligion is ForgeTongueBrotherhood && rm.CurrentReligion.CurrentFaithLevel >= 4)
        {
            if (item.Slot == EquipmentSlot.Weapon) return false;
        }

        int chance = 0;
        switch (item.Rarity)
        {
            case Rarity.Common: chance = 5; break;
            case Rarity.Uncommon: chance = 3; break;
            case Rarity.Rare: chance = 1; break;
            case Rarity.Epic: chance = 1; break;
            case Rarity.Legendary: chance = 0; break;
        }

        if (chance <= 0) return false;
        return UnityEngine.Random.Range(0, 100) < chance;
    }

    public void DegradeEquippedWeapons()
    {
        List<EquipmentSlot> slotsToBreak = new List<EquipmentSlot>();
        bool didDegrade = false;

        if (EquippedItems.TryGetValue(EquipmentSlot.Weapon, out Equipment weapon))
        {
            if (RollDurabilityDegradeChance(weapon))
            {
                didDegrade = true;
                if (weapon.DegradeEquipment(2)) slotsToBreak.Add(EquipmentSlot.Weapon);
            }
        }

        if (EquippedItems.TryGetValue(EquipmentSlot.OffHand, out Equipment offhand))
        {
            if (RollDurabilityDegradeChance(offhand))
            {
                didDegrade = true;
                if (offhand.DegradeEquipment(2)) slotsToBreak.Add(EquipmentSlot.OffHand);
            }
        }

        foreach (var slot in slotsToBreak)
        {
            TextOutputter.Instance.OutputText($"<color=red>Your {EquippedItems[slot].GetName()} broke!</color>");
            EquippedItems.Remove(slot); 
        }

        if (didDegrade)
        {
            RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
            Notify(EventType.EquipmentChanged);
        }
    }

    public void DegradeEquippedArmor()
    {
        List<EquipmentSlot> slotsToBreak = new List<EquipmentSlot>();
        bool didDegrade = false;

        EquipmentSlot[] armorSlots = { 
            EquipmentSlot.Head, EquipmentSlot.Chest, EquipmentSlot.Legs, 
            EquipmentSlot.Hands, EquipmentSlot.Belt, 
            EquipmentSlot.Accessory1, EquipmentSlot.Accessory2 
        };
        
        foreach (var slot in armorSlots)
        {
            if (EquippedItems.TryGetValue(slot, out Equipment armorPiece))
            {
                if (RollDurabilityDegradeChance(armorPiece))
                {
                    didDegrade = true;
                    if (armorPiece.DegradeEquipment(5)) slotsToBreak.Add(slot);
                }
            }
        }

        foreach (var slot in slotsToBreak)
        {
            TextOutputter.Instance.OutputText($"<color=red>Your {EquippedItems[slot].GetName()} broke!</color>");
            EquippedItems.Remove(slot);
        }

        if (didDegrade)
        {
            RunManager.Instance.GetService<PlayerManager>()?.RefreshEquipmentStats();
            Notify(EventType.EquipmentChanged);
        }
    }
}
