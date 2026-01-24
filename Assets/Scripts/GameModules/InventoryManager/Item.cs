using UnityEngine;

public class Item
{
    public virtual void Use()
    {
        Debug.Log("Using item: ");
    }

    public bool CanBeEquipped()
    {
        return false;
    }

    public bool CanBeUnequipped()
    {
        return false;
    }

    public bool CanBeUsedInCombat()
    {
        return false;
    }

    public bool IsTwoHanded()
    {
        return false;
    }
}
