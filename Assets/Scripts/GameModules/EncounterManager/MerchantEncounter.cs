using UnityEngine;
using System.Collections.Generic;

public class MerchantEncounter : Encounter
{
    private enum ShopState { Main, Buying }
    private ShopState _state = ShopState.Main;

    private struct ShopItem
    {
        public string ID;
        public string DisplayName;
        public int Price;
        public ShopItem(string id, string name, int price) { ID = id; DisplayName = name; Price = price; }
    }

    private List<ShopItem> _inventory = new List<ShopItem>
    {
        new ShopItem("Health PotionSO", "Health Potion", 25),
        new ShopItem("Mana PotionSO", "Mana Potion", 25),
        new ShopItem("Green HerbSO", "Green Herb", 10),
        new ShopItem("Blue HerbSO", "Blue Herb", 10),
        new ShopItem("Iron Key", "Iron Key", 50),
        new ShopItem("Lockpick", "Lockpick", 25)
    };

    public MerchantEncounter(float difficulty) : base(difficulty) { }

    public List<SerializableShopItem> GetSerializedInventory()
    {
        var list = new List<SerializableShopItem>();
        foreach (var item in _inventory)
        {
            list.Add(new SerializableShopItem { ID = item.ID, DisplayName = item.DisplayName, Price = item.Price, Rarity = Rarity.Common, Sold = false });
        }
        return list;
    }

    public void RestoreState(List<SerializableShopItem> savedStock)
    {
        if (savedStock == null || savedStock.Count == 0) return;
        _inventory.Clear();
        foreach (var s in savedStock)
        {
            _inventory.Add(new ShopItem(s.ID, s.DisplayName, s.Price));
        }
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        TextOutputter.Instance.OutputText("A hooded merchant beckons you. 'Looking for supplies?'");
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _state = ShopState.Main;
        List<string> choices = new List<string> { "Shop", "Leave" };
        new Prompt("Merchant Options:", choices, this);
    }

    private void ShowShopMenu()
    {
        _state = ShopState.Buying;
        List<string> choices = new List<string>();
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        int currentGold = em?.GetCurrentGold() ?? 0;

        foreach (var item in _inventory)
        {
            choices.Add($"{item.DisplayName} ({item.Price}g)");
        }
        choices.Add("Back");

        new Prompt($"Current Gold: {currentGold}g\nWhat would you like to buy?", choices, this);
    }

    public override void RecieveDecision(int decisionIndex)
    {
        if (_state == ShopState.Main)
        {
            if (decisionIndex == 0) ShowShopMenu();
            else ResolveEncounter();
        }
        else if (_state == ShopState.Buying)
        {
            if (decisionIndex < _inventory.Count)
            {
                BuyItem(_inventory[decisionIndex]);
            }
            else
            {
                ShowMainMenu();
            }
        }
    }

    private void BuyItem(ShopItem item)
    {
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (em != null && em.CanSpendGold(item.Price))
        {
            em.SpendGold(item.Price);
            Item newItem = ItemFactory.CreateItemByID(item.ID);
            RunManager.Instance.GetService<InventoryManager>()?.AddItemToInventory(newItem);
        }
        else
        {
            TextOutputter.Instance.OutputText("'You haven't got enough gold for that!'");
        }
        
        ShowShopMenu(); // Loop back
    }
}
