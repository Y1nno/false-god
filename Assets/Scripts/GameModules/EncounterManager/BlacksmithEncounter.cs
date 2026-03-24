using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BlacksmithEncounter : Encounter
{
    private enum BlacksmithState { Main, Smithing, Repairing, Buying }
    private BlacksmithState _state = BlacksmithState.Main;
    private List<Equipment> _upgradeableItems = new List<Equipment>();
    private List<Equipment> _repairableItems = new List<Equipment>();

    private struct ShopItem
    {
        public string ID;
        public string DisplayName;
        public int Price;
        public Rarity Rarity;
        public bool Sold;
        public ShopItem(string id, string name, int price, Rarity rarity) 
        { 
            ID = id; DisplayName = name; Price = price; Rarity = rarity; Sold = false;
        }
    }
    private List<ShopItem> _shopInventory = new List<ShopItem>();

    public BlacksmithEncounter(float difficulty) : base(difficulty) { }

    public override void StartEncounter()
    {
        base.StartEncounter();
        GenerateShopInventory();
        TextOutputter.Instance.OutputText("The heat of the forge hits you. 'Bring me your steel,' the blacksmith grunts.");
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _state = BlacksmithState.Main;
        List<string> choices = new List<string> { "Smith", "Repair", "Buy Items", "Leave" };
        new Prompt("Blacksmith Options:", choices, this);
    }

    private void ShowSmithMenu()
    {
        _state = BlacksmithState.Smithing;
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        _upgradeableItems = eqm.EquippedItems.Values.Where(item => item.UpgradeLevel < 10 && item.GetUpgradeCost() > 0).ToList();

        if (_upgradeableItems.Count == 0)
        {
            TextOutputter.Instance.OutputText("'You have no equipment to smith.'");
            ShowMainMenu();
            return;
        }

        List<string> choices = new List<string>();
        foreach (var item in _upgradeableItems)
        {
            int cost = item.GetUpgradeCost();
            string req = item.UpgradeLevel >= 5 ? " + Gemshard" : "";
            choices.Add($"{item.GetName()} -> {item.UpgradeLevel + 1} ({cost}g{req})");
        }
        choices.Add("Refresh List");
        choices.Add("Back");

        new Prompt("Select an item to upgrade:", choices, this);
    }

    private void ShowRepairMenu()
    {
        _state = BlacksmithState.Repairing;
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        
        // Show everything that isn't at full durability. 
        _repairableItems = eqm.EquippedItems.Values.Where(item => item.CurrentDurability < item.MaxDurability).ToList();

        if (_repairableItems.Count == 0)
        {
            TextOutputter.Instance.OutputText("'Everything you're wearing looks fine to me.'");
            ShowMainMenu();
            return;
        }

        List<string> choices = new List<string>();
        foreach (var item in _repairableItems)
        {
            int cost = GetRepairCost(item.Rarity);
            choices.Add($"{item.GetName()} ({item.CurrentDurability}/{item.MaxDurability}) - {cost}g");
        }
        choices.Add("Back");

        new Prompt("Select an item to repair:", choices, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (_state == BlacksmithState.Main)
        {
            if (decisionIndex == 0) ShowSmithMenu();
            else if (decisionIndex == 1) ShowRepairMenu();
            else if (decisionIndex == 2) ShowBuyMenu();
            else ResolveEncounter();
        }
        else if (_state == BlacksmithState.Smithing)
        {
            if (decisionIndex < _upgradeableItems.Count)
            {
                UpgradeItem(_upgradeableItems[decisionIndex]);
            }
            else if (decisionIndex == _upgradeableItems.Count)
            {
                ShowSmithMenu(); // Refresh
            }
            else
            {
                ShowMainMenu();
            }
        }
        else if (_state == BlacksmithState.Buying)
        {
            List<ShopItem> available = _shopInventory.Where(i => !i.Sold).ToList();
            if (decisionIndex < available.Count)
            {
                BuyItem(available[decisionIndex]);
            }
            else
            {
                ShowMainMenu();
            }
        }
    }

    private void UpgradeItem(Equipment item)
    {
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
        int cost = item.GetUpgradeCost();

        if (em == null || !em.CanSpendGold(cost))
        {
            TextOutputter.Instance.OutputText("'You don't have enough gold for this upgrade.'");
            ShowSmithMenu();
            return;
        }

        // Check for Gemshard if level >= 5
        MaterialInstance gemshard = null;
        if (item.UpgradeLevel >= 5)
        {
            gemshard = inv?.UnEquippedItems.Find(i => i is MaterialInstance m && m.GetName() == "Gem Shard") as MaterialInstance;
            if (gemshard == null)
            {
                TextOutputter.Instance.OutputText("'You need a Gemshard to upgrade this item further.'");
                ShowSmithMenu();
                return;
            }
        }

        // Deduct resources
        em.SpendGold(cost);
        if (gemshard != null)
        {
            if (gemshard.Quantity > 1) gemshard.Quantity--;
            else inv.RemoveItemFromInventory(gemshard);
        }

        // Upgrade
        item.UpgradeLevel++;
        TextOutputter.Instance.OutputText($"Clang! Your {item.BaseItemName} is now +{item.UpgradeLevel}.");
        
        // Refresh stats and UI
        eqm.RefreshEquipment();
        
        ShowSmithMenu(); // Loop back
    }

    private int GetRepairCost(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 15,
            Rarity.Epic => 20,
            Rarity.Legendary => 25,
            _ => 5
        };
    }

    private void ShowBuyMenu()
    {
        _state = BlacksmithState.Buying;
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        int currentGold = em?.GetCurrentGold() ?? 0;

        List<string> choices = new List<string>();
        foreach (var item in _shopInventory)
        {
            if (item.Sold) continue;
            choices.Add($"{item.DisplayName} [{item.Rarity}] ({item.Price}g)");
        }
        choices.Add("Back");

        new Prompt($"Current Gold: {currentGold}g\n'Take a look at my finished works.'", choices, this);
    }

    private void BuyItem(ShopItem shopItem)
    {
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();

        if (em == null || !em.CanSpendGold(shopItem.Price))
        {
            TextOutputter.Instance.OutputText("'You haven't got enough gold for that item.'");
            ShowBuyMenu();
            return;
        }

        em.SpendGold(shopItem.Price);
        Item newItem = ItemFactory.CreateItemByID(shopItem.ID);
        invm?.AddItemToInventory(newItem);
        
        // Mark as sold
        for (int i = 0; i < _shopInventory.Count; i++)
        {
            if (_shopInventory[i].ID == shopItem.ID && !_shopInventory[i].Sold)
            {
                ShopItem updated = _shopInventory[i];
                updated.Sold = true;
                _shopInventory[i] = updated;
                break;
            }
        }

        TextOutputter.Instance.OutputText($"You purchased the {shopItem.DisplayName}.");
        ShowBuyMenu();
    }

    private void GenerateShopInventory()
    {
        _shopInventory.Clear();
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int floor = dm?.CurrentDungeonFloor ?? 1;

        for (int i = 0; i < 5; i++)
        {
            Rarity rarity = RollShopRarity(floor);
            EquipmentSO eqSO = GetRandomEquipmentByRarity(rarity);
            if (eqSO != null)
            {
                int price = CalculatePrice(rarity);
                _shopInventory.Add(new ShopItem(eqSO.ItemID != "" ? eqSO.ItemID : eqSO.name, eqSO.ItemName, price, rarity));
            }
        }
    }

    private Rarity RollShopRarity(int floor)
    {
        float roll = Random.Range(0f, 100f);
        if (floor <= 10) 
        {
            return roll <= 90f ? Rarity.Common : Rarity.Uncommon;
        } 
        else if (floor <= 20) 
        {
            if (roll <= 65f) return Rarity.Common;
            if (roll <= 90f) return Rarity.Uncommon;
            return Rarity.Rare;
        } 
        else if (floor <= 30) 
        {
            if (roll <= 40f) return Rarity.Common;
            if (roll <= 80f) return Rarity.Uncommon;
            if (roll <= 95f) return Rarity.Rare;
            return Rarity.Legendary;
        } 
        else 
        {
            if (roll <= 25f) return Rarity.Common;
            if (roll <= 65f) return Rarity.Uncommon;
            if (roll <= 90f) return Rarity.Rare;
            return Rarity.Legendary;
        }
    }

    private int CalculatePrice(Rarity rarity)
    {
        int basePrice = rarity switch
        {
            Rarity.Common => 50,
            Rarity.Uncommon => 150,
            Rarity.Rare => 400,
            Rarity.Legendary => 1000,
            _ => 50
        };
        // Add +/- 10% variance
        float variance = Random.Range(0.9f, 1.1f);
        return Mathf.RoundToInt(basePrice * variance);
    }

    private EquipmentSO GetRandomEquipmentByRarity(Rarity rarity)
    {
        List<EquipmentSO> matching = new List<EquipmentSO>();
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EquipmentSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentSO>(path);
            if (so != null && so.Rarity == rarity) matching.Add(so);
        }
#else
        EquipmentSO[] allEquips = Resources.LoadAll<EquipmentSO>("");
        foreach (var eq in allEquips)
        {
            if (eq.Rarity == rarity) matching.Add(eq);
        }
#endif
        if (matching.Count > 0) return matching[Random.Range(0, matching.Count)];
        return null;
    }
}
