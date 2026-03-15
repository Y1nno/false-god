using UnityEngine;

public class MaterialInstance : Item
{
    public MaterialSO BaseData { get; private set; }
    public int Quantity { get; set; } = 1;

    public MaterialInstance(MaterialSO data, int quantity = 1)
    {
        BaseData = data;
        Quantity = quantity;
    }

    public override void Use()
    {
        TextOutputter.Instance.OutputText($"Materials like {BaseData.ItemName} cannot be used directly. They are for crafting or selling.");
    }
}
