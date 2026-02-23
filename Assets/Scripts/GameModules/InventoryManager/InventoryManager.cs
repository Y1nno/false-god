using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : GameModule
{
    private EquipmentManager _eq = new EquipmentManager();
    private Inventory _inv = new Inventory();

    #region Public API

    #region Equipment Methods

    public override void AttachDefaultObservers()
    {
        // none for now
    }
    #endregion
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
