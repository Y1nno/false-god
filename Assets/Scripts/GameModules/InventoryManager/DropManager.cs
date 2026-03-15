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
        _genericDrops.Add(new DropData("Broken gemshard", 0.1f));
        _genericDrops.Add(new DropData("Torn paper", 0.1f));
        _genericDrops.Add(new DropData("Paper", 0.1f));
        _genericDrops.Add(new DropData("Ink", 0.1f));
        _genericDrops.Add(new DropData("Iron Key", 0.1f));
        _genericDrops.Add(new DropData("Lockpick", 0.1f));
    }

    public override void AttachDefaultObservers()
    {
        // We'll be manually attached to enemies or via a global event if possible
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EnemyDefeated && subject is Enemy enemy)
        {
            RollDrops(enemy.GetName());
        }
    }

    private void RollDrops(string enemyName)
    {
        // TODO: Specific enemy drop check
        // For now, as requested, every enemy drops them, but we'll include the specific ones too for future proofing.
        
        // TODO: Relics should only drop from Unique Enemies and Bosses (Bosses not yet implemented)
        // Roll Generic Drops
        foreach (var drop in _genericDrops)
        {
            TryDrop(drop);
        }

        // Roll Specific Drops
        foreach (var table in _dropTables)
        {
            foreach (var drop in table.Value)
            {
                // TODO: Check if enemyName == table.Key once more enemies are added
                TryDrop(drop);
            }
        }
    }

    private void TryDrop(DropData drop)
    {
        if (Random.value <= drop.Chance)
        {
            int qty = Random.Range(drop.MinQty, drop.MaxQty + 1);
            Item newItem = ItemFactory.CreateItemByID(drop.ItemID);
            if (newItem != null)
            {
                if (newItem is MaterialInstance mat) mat.Quantity = qty;
                RunManager.Instance.GetService<InventoryManager>()?.AddItemToInventory(newItem);
            }
        }
    }
}
