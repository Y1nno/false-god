using UnityEngine;

public class ColossusRite : Rite
{
    private const int k_DamageBonus = 3;

    public override string Description => $"+{k_DamageBonus} Damage";

    public ColossusRite() : base("Colossus", 2) // ID: Colossus, Cost: 2
    {
        RiteType = RiteType.Colossus;
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Make this conditional on holding a "Heavy" weapon (Hammer, Greatsword, etc.)
        // 1. Make ColossusRite implement IObserver.
        // 2. Subscribe to InventoryManager (RunManager.Instance.GetService<InventoryManager>()).
        // 3. InventoryManager needs to Notify(EventType.EquipmentChanged) when equipping/unequipping.
        // 4. OnNotify, check player.Inventory.GetEquippedItem(EquipmentSlot.Weapon).ItemType.
        // 5. If heavy, apply BonusDamage. If not, remove it.
        player.BonusDamage += k_DamageBonus;
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Unsubscribe from InventoryManager
        player.BonusDamage -= k_DamageBonus;
    }

    // TODO: Implement OnNotify to handle equipment changes dynamically.
}
