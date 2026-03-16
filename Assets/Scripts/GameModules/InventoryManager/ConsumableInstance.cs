using UnityEngine;

public class ConsumableInstance : Item
{
    public Consumable BaseData { get; private set; }
    public override string GetName() => BaseData != null ? BaseData.ItemName : "Unknown Consumable";
    public int Tier { get; private set; }

    public ConsumableInstance(Consumable baseData, int tier)
    {
        BaseData = baseData;
        Tier = Mathf.Clamp(tier, 1, 3);
    }

    public override void Use()
    {
        // For ConsumableInstance, the inventory or UI normally calls this.
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        
        // We can use consumables anytime as long as the PCM exists
        if (cm != null && cm.Pcm != null)
        {
            Use(cm.Pcm);
            return;
        }
        
        TextOutputter.Instance.OutputText("Cannot use consumable: Player state not found.");
    }

    public void Use(Combatant target)
    {
        if (BaseData != null)
        {
            BaseData.Use(target, Tier);
        }
    }
}
