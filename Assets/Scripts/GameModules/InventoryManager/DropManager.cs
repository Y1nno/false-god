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
            RollDrops(enemy);
        }
    }

    private void RollDrops(Enemy enemy)
    {
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        EconomyManager economy = RunManager.Instance.GetService<EconomyManager>();
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (invm == null || dm == null) return;

        int floor = dm.CurrentDungeonFloor;

        if (enemy is Boss boss)
        {
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
            return; // Bosses have their own fixed drops, skipping global pool for now unless governed otherwise
        }

        // --- GLOBAL DROP CATEGORY ROLL ---
        float roll = Random.Range(0f, 100f);
        
        int totalMF = eqm != null ? eqm.GetTotalMagicFind() : 0;
        float mfMultiplier = 1.0f + (totalMF / 100.0f);

        // 1. No Drop (50% Base)
        // Adjust no-drop threshold. If base is 50%, then 50% chance to find something.
        // With 20 MF, chance to find something becomes 50% * 1.2 = 60%.
        // No-drop threshold becomes 100 - 60 = 40.
        float baseTotalDropChance = 50f;
        float adjustedTotalDropChance = Mathf.Min(100f, baseTotalDropChance * mfMultiplier);
        float noDropThreshold = 100f - adjustedTotalDropChance;

        if (roll <= noDropThreshold) return;

        // 2. Gold (5%)
        float goldThreshold = noDropThreshold + (5f * mfMultiplier);
        if (roll <= goldThreshold) { RollGold(floor, economy); return; }

        // 3. Consumables (10%)
        float consumablesThreshold = goldThreshold + (10f * mfMultiplier);
        if (roll <= consumablesThreshold) { RollConsumables(floor, invm); return; }

        // 4. Materials (10%)
        float materialsThreshold = consumablesThreshold + (10f * mfMultiplier);
        if (roll <= materialsThreshold) { RollMaterials(enemy, invm); return; }

        // 5. Equipment (10%)
        float equipmentThreshold = materialsThreshold + (10f * mfMultiplier);
        if (roll <= equipmentThreshold) { RollEquipment(floor, invm); return; }

        // 6. Quest Item (3.5%)
        float questThreshold = equipmentThreshold + (3.5f * mfMultiplier);
        if (roll <= questThreshold) { RollQuestItem(invm, enemy); return; }

        // 7. Relic (1.5% Base, 10% if Unique)
        float baseRelicChance = enemy.IsUnique ? 10.0f : 1.5f;
        float relicChance = baseRelicChance * mfMultiplier;
        float relicThreshold = questThreshold + relicChance;
        if (roll <= relicThreshold) { RollRelic(invm); return; }

        // 8. Key Item (Remaining)
        RollKeyItem(invm);
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
            else itemID = "Enchanted Berry"; // 2 (Wait, sum is 50. 48 to 50 is 2)
        }

        SpawnAndGive(itemID, invm, tier);
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

    private void RollEquipment(int floor, InventoryManager invm)
    {
        Rarity targetRarity = Rarity.Common;
        float roll = Random.Range(0f, 100f);

        if (floor <= 10) {
            targetRarity = roll <= 85f ? Rarity.Common : Rarity.Uncommon;
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
