using UnityEngine;

public class RelicInstance : Item
{
    public RelicSO BaseData { get; private set; }

    public RelicInstance(RelicSO so)
    {
        BaseData = so;
    }

    public override void Use()
    {
        TextOutputter.Instance.OutputText($"{BaseData.ItemName} is a passive relic and cannot be used manually.");
    }
}
