using UnityEngine;

public class SpellScroll : Item
{
    public SpellSO Spell;
    public int ItemID;

    public SpellScroll(SpellSO spell, int itemID)
    {
        Spell = spell;
        ItemID = itemID;
    }

    public override string GetName() => $"Scroll of {Spell.actionName}";

    public override void Use()
    {
        // Logic moved to InventoryManager/SpellManager
    }
}
