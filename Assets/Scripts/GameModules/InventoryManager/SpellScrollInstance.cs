using UnityEngine;

public class SpellScrollInstance : Item
{
    public SpellScrollSO BaseData { get; private set; }
    public override string GetName() => BaseData != null ? BaseData.ItemName : "Unknown Spell Scroll";

    public SpellScrollInstance(SpellScrollSO baseData)
    {
        BaseData = baseData;
    }

    public override void Use()
    {
        TextOutputter.Instance.OutputText("To learn this spell, use the 'Learn' button in your Spellbook.");
    }
}
