using UnityEngine;
using System.Collections.Generic;

public class Inventory
{
    private List<Item> _inventory = new List<Item>();
    private int _maxCapacity = 10;

    public void AddItem(Item item)
    {
        if (TryAddItem(item) == false)
        {
            Debug.LogWarning("Inventory is full. Cannot add item.");
            return;
        }
        _inventory.Add(item);
    }

    public void AddItem(Item item, int position)
    {
        if (position < 0 || position > _inventory.Count)
        {
            throw new System.ArgumentOutOfRangeException("position", "Position is out of range.");
        }
        if (_inventory[position] == item)
        {
            _inventory.Add(item);
        }
        if (position == _inventory.Count)
        {
            _inventory.Add(item);
        }
        else
        {
            _inventory.Insert(position, item);
        }
    }

    public Item RemoveItem(Item item)
    {
        if (_inventory.Contains(item))
        {
            _inventory.Remove(item);
            return item;
        }
        return null;
    }

    public void DiscardItem(Item item)
    {
        if (_inventory.Contains(item))
        {
            _inventory.Remove(item);
        }
    }
    
    public bool ContainsItem(Item item)
    {
        return _inventory.Contains(item);
    }

    public int GetItemCount(int itemID)
    {
        int count = 0;
        foreach (Item item in _inventory)
        {
            if (item != null && item.ID == itemID)
            {
                count++;
            }
        }
        return count;
    }

    public List<Item> GetAllItems()
    {
        return _inventory;
    }

    private bool TryAddItem(Item item)
    {
        int count = 0;
        foreach (Item invItem in _inventory)
        {
            if (invItem == null)
            {
                count++;
            }
        }
        return count < _maxCapacity;
    }
}
