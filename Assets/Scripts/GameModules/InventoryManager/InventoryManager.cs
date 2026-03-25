using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//TODO: Add inventory size limit and logic for refusing requests to add to inventory. 

public class InventoryManager : GameModule, IPromptResponder
{
    private EquipmentManager _eq => RunManager.Instance.GetService<EquipmentManager>();
    private Inventory _inv = new Inventory();
    private Equipment _itemPendingEquip;

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

    public void RestoreState(List<SerializableItem> items)
    {
        UnEquippedItems.Clear();
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        
        foreach (var sItem in items)
        {
            Item item = null;
            if (sItem.ItemType == "Equipment")
            {
                item = eqm?.ReconstructEquipment(sItem);
            }
            else if (sItem.ItemType == "Consumable")
            {
                Consumable[] all = Resources.LoadAll<Consumable>("Items");
                Consumable baseData = System.Array.Find(all, so => so.ItemID == sItem.ItemID);
                if (baseData != null) item = new ConsumableInstance(baseData, sItem.Tier);
            }
            else if (sItem.ItemType == "Material")
            {
                MaterialSO[] all = Resources.LoadAll<MaterialSO>("Items");
                MaterialSO baseData = System.Array.Find(all, so => so.ItemID == sItem.ItemID);
                if (baseData != null) item = new MaterialInstance(baseData, sItem.Quantity);
            }
            else if (sItem.ItemType == "Key")
            {
                KeySO[] all = Resources.LoadAll<KeySO>("Items");
                KeySO baseData = System.Array.Find(all, so => so.ItemID == sItem.ItemID);
                if (baseData != null) 
                {
                    KeyInstance key = new KeyInstance(baseData, sItem.Quantity);
                    key.RemainingUses = sItem.RemainingUses;
                    item = key;
                }
            }
            else if (sItem.ItemType == "Relic")
            {
                RelicSO[] all = Resources.LoadAll<RelicSO>("Items");
                RelicSO baseData = System.Array.Find(all, so => so.ItemID == sItem.ItemID);
                if (baseData != null) item = new RelicInstance(baseData);
            }

            if (item != null)
            {
                UnEquippedItems.Add(item);
            }
        }
        Notify(EventType.ItemAcquired);
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

    public bool HasItem(string itemID)
    {
        return UnEquippedItems.Any(i => 
        {
            if (i is MaterialInstance m && m.BaseData != null) return m.BaseData.ItemID == itemID;
            if (i is Equipment e && e.BaseData != null) return e.BaseData.ItemID == itemID;
            if (i is ConsumableInstance c && c.BaseData != null) return c.BaseData.ItemID == itemID;
            return false;
        });
    }

    public void RemoveItemByID(string itemID)
    {
        Item item = UnEquippedItems.Find(i => 
        {
            if (i is MaterialInstance m && m.BaseData != null) return m.BaseData.ItemID == itemID;
            if (i is Equipment e && e.BaseData != null) return e.BaseData.ItemID == itemID;
            if (i is ConsumableInstance c && c.BaseData != null) return c.BaseData.ItemID == itemID;
            return false;
        });

        if (item != null)
        {
            RemoveItemFromInventory(item);
        }
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

        // Consolidation Logic: Merge MaterialInstances if they are identical and under stack limit
        if (item is MaterialInstance newMat)
        {
            MaterialInstance existing = UnEquippedItems.Find(i => i is MaterialInstance m && m.BaseData == newMat.BaseData && m.Quantity < m.BaseData.MaxStackSize) as MaterialInstance;
            if (existing != null)
            {
                int canAdd = existing.BaseData.MaxStackSize - existing.Quantity;
                int toAdd = Mathf.Min(canAdd, newMat.Quantity);
                existing.Quantity += toAdd;
                newMat.Quantity -= toAdd;

                if (newMat.Quantity <= 0)
                {
                    TextOutputter.Instance.OutputText($"Added {newMat.GetName()} to Inventory stack.");
                    Notify(EventType.ItemAcquired);
                    return;
                }
            }
        }

        // Consolidation Logic: Merge KeyInstances if they are identical and under stack limit
        if (item is KeyInstance newKey)
        {
            KeyInstance existing = UnEquippedItems.Find(i => i is KeyInstance k && k.BaseData == newKey.BaseData && k.Quantity < k.BaseData.MaxStackSize) as KeyInstance;
            if (existing != null)
            {
                int canAdd = existing.BaseData.MaxStackSize - existing.Quantity;
                int toAdd = Mathf.Min(canAdd, newKey.Quantity);
                existing.Quantity += toAdd;
                newKey.Quantity -= toAdd;

                if (newKey.Quantity <= 0)
                {
                    TextOutputter.Instance.OutputText($"Added {newKey.GetName()} to Inventory stack.");
                    Notify(EventType.ItemAcquired);
                    return;
                }
            }
        }

        UnEquippedItems.Add(item);
        
        string itemName = item.GetName();
                          
        TextOutputter.Instance.OutputText($"Added {itemName} to Inventory.");
        Notify(EventType.ItemAcquired);
        RunManager.Instance.GetService<SaveManager>()?.SaveRun();
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (UnEquippedItems.Contains(item))
        {
            UnEquippedItems.Remove(item);
            Notify(EventType.ItemRemoved);
            RunManager.Instance.GetService<SaveManager>()?.SaveRun();
        }
    }

    // Handles the UI passing down requests to either wear a weapon or drink a potion
    public void UseOrEquipItem(Item itemToHandle)
    {
        if (itemToHandle == null || !UnEquippedItems.Contains(itemToHandle)) return;

        if (itemToHandle is Equipment equipment)
        {
            // Handle Accessory Smart Slotting
            if (equipment.Slot == EquipmentSlot.Accessory1 || equipment.Slot == EquipmentSlot.Accessory2)
            {
                Equipment eq1 = _eq.GetEquippedItem(EquipmentSlot.Accessory1);
                Equipment eq2 = _eq.GetEquippedItem(EquipmentSlot.Accessory2);

                if (eq1 == null)
                {
                    _eq.EquipItemToSlot(equipment, EquipmentSlot.Accessory1);
                    RemoveItemFromInventory(equipment);
                }
                else if (eq2 == null)
                {
                    _eq.EquipItemToSlot(equipment, EquipmentSlot.Accessory2);
                    RemoveItemFromInventory(equipment);
                }
                else
                {
                    // Both slots full, ask user
                    _itemPendingEquip = equipment;
                    List<string> options = new List<string> { 
                        $"Replace {eq1.GetName()} (Slot 1)", 
                        $"Replace {eq2.GetName()} (Slot 2)",
                        "Cancel" 
                    };
                    new Prompt("Which accessory slot would you like to use?", options, this);
                }
            }
            else
            {
                // Normal Equipment
                _eq.EquipItem(equipment);
                RemoveItemFromInventory(equipment);
            }
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

    #region IPromptResponder
    public void ProcessPromptResponse(int decisionIndex)
    {
        if (_itemPendingEquip == null) return;

        if (decisionIndex == 0) // Slot 1
        {
            _eq.EquipItemToSlot(_itemPendingEquip, EquipmentSlot.Accessory1);
            RemoveItemFromInventory(_itemPendingEquip);
        }
        else if (decisionIndex == 1) // Slot 2
        {
            _eq.EquipItemToSlot(_itemPendingEquip, EquipmentSlot.Accessory2);
            RemoveItemFromInventory(_itemPendingEquip);
        }
        // else 2 is Cancel, do nothing

        _itemPendingEquip = null;
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
