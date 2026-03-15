using UnityEngine;

public class KeyInstance : Item
{
    public KeySO BaseData { get; private set; }
    public int RemainingUses { get; set; }

    public KeyInstance(KeySO data)
    {
        BaseData = data;
        RemainingUses = data.MaxUses;
    }

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
