using UnityEngine;

public class KeyInstance : Item
{
    public KeySO BaseData { get; private set; }
    public int RemainingUses { get; set; }
    public int Quantity { get; set; } = 1;

    public KeyInstance(KeySO data, int quantity = 1)
    {
        BaseData = data;
        RemainingUses = data.MaxUses;
        Quantity = quantity;
    }
    public override string GetName() => (BaseData != null ? BaseData.ItemName : "Unknown Key") + (Quantity > 1 ? $" x{Quantity}" : "");
    public override void Use()
    {
        TextOutputter.Instance.OutputText($"The {BaseData.ItemName} is used contextualy on chests. You cannot use it from the menu.");
    }

    public bool TryUse()
    {
        if (Random.value <= BaseData.SuccessChance)
        {
            RemainingUses--;
            return true;
        }
        
        // Lockpicks still get consumed even on failure if they only have 1 use
        RemainingUses--;
        return false;
    }
}
