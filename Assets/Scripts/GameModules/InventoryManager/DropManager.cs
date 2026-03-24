using UnityEngine;
using System.Collections.Generic;

public class DropManager : GameModule, IObserver
{
    public override void AttachDefaultObservers()
    {
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EnemyDefeated && subject is Enemy enemy)
        {
            RollMonsterDrops(enemy);
        }
    }

    public void RollChestDrops(int floor)
    {
        EconomyManager economy = RunManager.Instance.GetService<EconomyManager>();
        if (economy != null) RollGold(floor, economy); // Guaranteed gold for chests

        int count = Random.Range(1, 4); // 1-3 items
        for (int i = 0; i < count; i++)
        {
            RollSingleDrop(floor, true, null);
        }
    }

    private void RollMonsterDrops(Enemy enemy)
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        if (dm == null) return;
        
        int floor = dm.CurrentDungeonFloor;

        if (enemy is Boss boss)
        {
            InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
            RollGuaranteedRelic(invm);
            foreach (string dropID in boss.BossData.DropIDs)
            {
                Item newItem = ItemFactory.CreateItemByID(dropID);
                if (newItem != null)
                {
                    if (newItem is RelicInstance rel && invm.HasRelic(rel.GetName())) continue;
                    invm.AddItemToInventory(newItem);
                    TextOutputter.Instance.OutputText($"{boss.GetName()} dropped {newItem.GetName()}!");
                }
            }
            return;
        }

        // --- GLOBAL DROP CATEGORY ROLL ---
        float roll = Random.Range(0f, 100f);
        float baseTotalDropChance = 60f; // 40% No Drop, 60% Total Drop
        float noDropThreshold = 100f - baseTotalDropChance;

        if (roll <= noDropThreshold) return;

        RollSingleDrop(floor, false, enemy);
    }

    private void RollSingleDrop(int floor, bool isChest, Enemy enemy = null)
    {
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        EconomyManager economy = RunManager.Instance.GetService<EconomyManager>();
        if (invm == null) return;

        // Total drop pool is 60%. We roll 0-60.
        float roll = Random.Range(0f, 60f);
        
        // Category Weights based on provided table
        if (roll <= 5f) { RollGold(floor, economy); return; }           // Gold: 5%
        if (roll <= 15f) { RollConsumables(floor, invm); return; }       // Consumables: 10%
        if (roll <= 25f) {                                              // Materials: 10%
            if (isChest) RollConsumables(floor, invm); 
            else if (enemy != null) RollMaterials(enemy, invm); 
            return; 
        }
        if (roll <= 35f) { RollEquipment(floor, invm, isChest); return; } // Equipment: 10%
        if (roll <= 45f) { RollSpellScroll(floor, invm); return; }       // Spells: 10%
        
        // Quest Item handling: 3.5% (20% if quest active)
        float questChance = 3.5f; 
        // TODO: check for active quest and set questChance = 20f
        if (roll <= 45f + questChance) { 
            if (isChest) RollEquipment(floor, invm, isChest);
            else if (enemy != null) RollQuestItem(invm, enemy);
            return;
        }

        // Relic handling: 1.5% (10% if unique)
        float relicChance = (enemy != null && enemy.IsUnique) ? 10f : 1.5f;
        if (roll <= 45f + questChance + relicChance) { RollRelic(invm); return; }
        
        RollKeyItem(invm); // Remaining chance up to 60 (approx 10%)
    }

    private void RollGold(int floor, EconomyManager economy)
    {
        if (economy == null) return;
        int gold = 0;
        if (floor <= 10) gold = Random.Range(5, 11);
        else if (floor <= 20) gold = Random.Range(25, 51);
        else if (floor <= 30) gold = Random.Range(50, 101);
        else gold = Random.Range(100, 131);

        economy.AddGold(gold);
        TextOutputter.Instance.OutputText($"Found {gold} gold!");
    }

    private void RollConsumables(int floor, InventoryManager invm)
    {
        bool isPotion = Random.value <= 0.5f;
        string itemID = "";
        int tier = 1;

        if (isPotion)
        {
            float potRoll = Random.value;
            if (potRoll <= 0.4f) itemID = "Health Potion";
            else if (potRoll <= 0.8f) itemID = "Mana Potion";
            else itemID = "Elixir";

            float tRoll = Random.value;
            if (floor <= 10) tier = (tRoll <= 0.9f) ? 1 : 2;
            else if (floor <= 20) tier = (tRoll <= 0.5f) ? 1 : 2;
            else if (floor <= 30) { if (tRoll <= 0.35f) tier = 1; else if (tRoll <= 0.8f) tier = 2; else tier = 3; }
            else { if (tRoll <= 0.2f) tier = 1; else if (tRoll <= 0.5f) tier = 2; else tier = 3; }
        }
        else
        {
            tier = 1; // Non-tiered
            float otherRoll = Random.Range(0f, 52f);
            if (otherRoll <= 10f) itemID = "Monster Meat"; // 10
            else if (otherRoll <= 15f) itemID = "Green Herb"; // 5
            else if (otherRoll <= 20f) itemID = "Blue Herb"; // 5
            else if (otherRoll <= 24f) itemID = "Empty Bottle"; // 4
            else if (otherRoll <= 29f) itemID = "Rotten Flesh"; // 5
            else if (otherRoll <= 34f) itemID = "Scroll of Power"; // 5
            else if (otherRoll <= 39f) itemID = "Scroll of Knowledge"; // 5
            else if (otherRoll <= 44f) itemID = "Scroll of Dexterity"; // 5
            else if (otherRoll <= 46f) itemID = "Scroll of Speed"; // 2
            else if (otherRoll <= 48f) itemID = "Scroll of Portal"; // 2
            else if (otherRoll <= 50f) itemID = "Enchanted Berry"; // 2
            else { RollSpellScroll(floor, invm); return; } // Remaining chance for Spells
        }

        SpawnAndGive(itemID, invm, tier);
    }

    private void RollSpellScroll(int floor, InventoryManager invm)
    {
        Rarity targetRarity = Rarity.Common;
        float roll = Random.Range(0f, 100f);

        if (floor <= 10) 
        {
            if (roll <= 85) targetRarity = Rarity.Common;
            else targetRarity = Rarity.Uncommon;
        }
        else if (floor <= 20) 
        {
            if (roll <= 50) targetRarity = Rarity.Common;
            else if (roll <= 85) targetRarity = Rarity.Uncommon;
            else targetRarity = Rarity.Rare;
        }
        else if (floor <= 30) 
        {
            if (roll <= 20) targetRarity = Rarity.Common;
            else if (roll <= 65) targetRarity = Rarity.Uncommon;
            else if (roll <= 95) targetRarity = Rarity.Rare;
            else targetRarity = Rarity.Legendary;
        }
        else 
        {
            if (roll <= 10) targetRarity = Rarity.Common;
            else if (roll <= 45) targetRarity = Rarity.Uncommon;
            else if (roll <= 90) targetRarity = Rarity.Rare;
            else targetRarity = Rarity.Legendary;
        }

        List<SpellScrollSO> matching = new List<SpellScrollSO>();
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpellScrollSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            SpellScrollSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellScrollSO>(path);
            if (so != null && so.Spell != null && so.Spell.Rarity == targetRarity) matching.Add(so);
        }
#endif
        if (matching.Count > 0)
        {
            SpellScrollSO selected = matching[Random.Range(0, matching.Count)];
            SpawnAndGive(selected.ItemID != "" ? selected.ItemID : selected.name, invm);
        }
        else
        {
            // Fallback to a common one if no matching rarity found (shouldn't happen with our 12)
            SpawnAndGive("146", invm); // Pyro Blast
        }
    }

    private void RollMaterials(Enemy enemy, InventoryManager invm)
    {
        string n = enemy.Name.ToLower();
        string itemID = "";

        if (n.Contains("skeleton"))
        {
            if (Random.value <= 0.2f) itemID = "Broken Bones";
        }
        else if (n.Contains("nullmite"))
        {
            itemID = "Gemshard"; // 100%
        }
        else if (n.Contains("rogue"))
        {
            itemID = "Gold Bag"; // 100%
        }
        else
        {
            float mRoll = Random.value;
            if (mRoll <= 0.2f) itemID = "Broken GemShard";
            else if (mRoll <= 0.5f) itemID = "Torn Paper";
            else if (mRoll <= 0.7f) itemID = "Paper";
            else if (mRoll <= 0.8f) itemID = "Ink";
            // remaining 20% is nothing
        }

        if (!string.IsNullOrEmpty(itemID)) SpawnAndGive(itemID, invm);
    }

    private void RollEquipment(int floor, InventoryManager invm, bool isChest = false)
    {
        Rarity targetRarity = Rarity.Common;
        float roll = Random.Range(0f, 100f);

        if (floor <= 10) {
            if (roll <= 85f) targetRarity = Rarity.Common;
            else targetRarity = Rarity.Uncommon;
        } else if (floor <= 20) {
            if (roll <= 50f) targetRarity = Rarity.Common;
            else if (roll <= 85f) targetRarity = Rarity.Uncommon;
            else targetRarity = Rarity.Rare;
        } else if (floor <= 30) {
            if (roll <= 20f) targetRarity = Rarity.Common;
            else if (roll <= 65f) targetRarity = Rarity.Uncommon;
            else if (roll <= 95f) targetRarity = Rarity.Rare;
            else targetRarity = Rarity.Legendary;
        } else {
            if (roll <= 10f) targetRarity = Rarity.Common;
            else if (roll <= 45f) targetRarity = Rarity.Uncommon;
            else if (roll <= 90f) targetRarity = Rarity.Rare;
            else targetRarity = Rarity.Legendary;
        }

        EquipmentSO selectedEq = GetRandomEquipmentByRarity(targetRarity);
        if (selectedEq != null)
        {
            SpawnAndGive(selectedEq.ItemID != "" ? selectedEq.ItemID : selectedEq.name, invm);
        }
    }

    private void RollQuestItem(InventoryManager invm, Enemy enemy)
    {
        // Base drop chance is 3.5%
        // TODO: If quest is online/active, chance should be 20%
        // The global category roll already handles the 3.5% chance (Category 6).
        // So any enemy that falls into this category should drop their item.

        string enemyName = enemy.GetName();
        string itemID = "";

        switch (enemyName)
        {
            case "Brute Ogre": itemID = "140"; break;
            case "Cloaker": itemID = "141"; break;
            case "Banshee": itemID = "142"; break;
            case "Kobold": itemID = "143"; break;
            case "Demon Centaur": itemID = "144"; break;
            case "Stone Golem": itemID = "145"; break;
            default:
                // Fallback for enemies without specific quest items
                TextOutputter.Instance.OutputText($"{enemyName} dropped nothing special...");
                return;
        }

        if (itemID != "")
        {
            SpawnAndGive(itemID, invm);
        }
    }

    private void RollRelic(InventoryManager invm)
    {
        // Generates a random non-duplicate relic
        RollGuaranteedRelic(invm);
    }

    private void RollKeyItem(InventoryManager invm)
    {
        string itemID = Random.value <= 0.4f ? "Iron Key" : "Lockpick";
        SpawnAndGive(itemID, invm);
    }

    private void SpawnAndGive(string itemID, InventoryManager invm, int tier = 1)
    {
         Item newItem = ItemFactory.CreateItemByID(itemID, tier);
         if (newItem != null)
         {
             invm.AddItemToInventory(newItem);
             TextOutputter.Instance.OutputText($"Dropped: {newItem.GetName()}!");
         }
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

    private void RollGuaranteedRelic(InventoryManager invm)
    {
        List<string> allRelicIDs = new List<string>();
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RelicSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            RelicSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<RelicSO>(path);
            if (so != null) allRelicIDs.Add(so.ItemID != "" ? so.ItemID : so.name);
        }
#else
        allRelicIDs.Add("Whispering Reliquary");
        allRelicIDs.Add("Shattered Halo Rare");
#endif
        List<string> availableRelics = new List<string>();
        foreach(string id in allRelicIDs)
        {
            Item temp = ItemFactory.CreateItemByID(id);
            if (temp != null && temp is RelicInstance rel)
            {
                if (!invm.HasRelic(rel.GetName())) availableRelics.Add(id);
            }
        }

        if (availableRelics.Count > 0)
        {
            string selectedID = availableRelics[Random.Range(0, availableRelics.Count)];
            Item finalRelic = ItemFactory.CreateItemByID(selectedID);
            if (finalRelic != null)
            {
                 invm.AddItemToInventory(finalRelic);
                 TextOutputter.Instance.OutputText($"Dropped a unique relic: {finalRelic.GetName()}!");
            }
        }
        else
        {
             TextOutputter.Instance.OutputText("You already possessed all available relics!");
        }
    }
}
