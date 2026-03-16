using UnityEngine;
using System.Collections.Generic;

public class DropManager : GameModule, IObserver
{
    private class DropData
    {
        public string ItemID;
        public float Chance;
        public int MinQty;
        public int MaxQty;

        public DropData(string id, float chance, int min = 1, int max = 1)
        {
            ItemID = id;
            Chance = chance;
            MinQty = min;
            MaxQty = max;
        }
    }

    private Dictionary<string, List<DropData>> _dropTables = new Dictionary<string, List<DropData>>();
    private List<DropData> _genericDrops = new List<DropData>();

    public DropManager()
    {
        InitializeDropTables();
    }

    private void InitializeDropTables()
    {
        // Specific Enemy Drops
        _dropTables["Nullmite"] = new List<DropData> { new DropData("Gemshard", 0.8f) };
        _dropTables["Skeletons"] = new List<DropData> { new DropData("Broken Bones", 0.2f) };
        _dropTables["Golbin Rogue"] = new List<DropData> { new DropData("115", 0.2f) };

        // Generic Drops (Drop from "Enemies")
        _genericDrops.Add(new DropData("Broken GemShard", 0.1f));
        _genericDrops.Add(new DropData("Broken Bones", 0.05f));
        _genericDrops.Add(new DropData("Skeleton Key", 0.02f));
    }

    public override void AttachDefaultObservers()
    {
        // We'll be manually attached to enemies or via a global event if possible
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
        if (invm == null) return;

        if (enemy is Boss boss)
        {
            // Guaranteed Unique Relic Drop
            RollGuaranteedRelic(invm);

            // Specific Boss Drops
            foreach (string dropID in boss.BossData.DropIDs)
            {
                Item newItem = ItemFactory.CreateItemByID(dropID);
                if (newItem != null)
                {
                    // Check for relic uniqueness if the drop is a relic
                    if (newItem is RelicInstance rel && invm.HasRelic(rel.GetName()))
                    {
                        continue;
                    }

                    invm.AddItemToInventory(newItem);
                    TextOutputter.Instance.OutputText($"{boss.GetName()} dropped {newItem.GetName()}!");
                }
            }
        }

        // Generic Drops
        foreach (var drop in _genericDrops)
        {
            TryDrop(drop, invm);
        }

        // Specific Drops
        foreach (var table in _dropTables)
        {
            foreach (var drop in table.Value)
            {
                TryDrop(drop, invm);
            }
        }
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
        // In a real build, you'd load from Resources or a pre-populated list
        // For now, let's use a small subset of known IDs if we're not in editor
        allRelicIDs.Add("Whispering Reliquary");
        allRelicIDs.Add("Shattered Halo Rare");
#endif

        // Filter out what we already have
        List<string> availableRelics = new List<string>();
        foreach(string id in allRelicIDs)
        {
            // We need to be careful with ID vs Name. ItemFactory uses both.
            // Let's create the item temporarily to check name, or just use ID if we're confident.
            Item temp = ItemFactory.CreateItemByID(id);
            if (temp != null && temp is RelicInstance rel)
            {
                if (!invm.HasRelic(rel.GetName()))
                {
                    availableRelics.Add(id);
                }
            }
        }

        if (availableRelics.Count > 0)
        {
            string selectedID = availableRelics[Random.Range(0, availableRelics.Count)];
            Item finalRelic = ItemFactory.CreateItemByID(selectedID);
            if (finalRelic != null)
            {
                 invm.AddItemToInventory(finalRelic);
                 TextOutputter.Instance.OutputText($"Boss dropped a unique relic: {finalRelic.GetName()}!");
            }
        }
        else
        {
             TextOutputter.Instance.OutputText("You already possessed all available relics!");
        }
    }

    private void TryDrop(DropData drop, InventoryManager invm)
    {
        if (Random.value <= drop.Chance)
        {
            int qty = Random.Range(drop.MinQty, drop.MaxQty + 1);
            Item newItem = ItemFactory.CreateItemByID(drop.ItemID);
            if (newItem != null)
            {
                // Uniqueness check for relics
                if (newItem is RelicInstance rel && invm.HasRelic(rel.GetName()))
                {
                    return;
                }

                if (newItem is MaterialInstance mat) mat.Quantity = qty;
                invm.AddItemToInventory(newItem);
            }
        }
    }
}
