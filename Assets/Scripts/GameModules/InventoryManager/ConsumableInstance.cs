using UnityEngine;

public class ConsumableInstance : Item
{
    public Consumable BaseData { get; private set; }
    public int Tier { get; private set; }

    public ConsumableInstance(Consumable baseData, int tier)
    {
        BaseData = baseData;
        Tier = Mathf.Clamp(tier, 1, 3);
    }

    public override void Use()
    {
        // For ConsumableInstance, the inventory or UI normally calls this.
        // We need a target; we assume Player for now if invoked directly without target.
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        if (cm != null && cm.CurrentBattle != null && cm.CurrentBattle.Pcm != null)
        {
            Use(cm.CurrentBattle.Pcm);
        }
        else
        {
            TextOutputter.Instance.OutputText("Cannot use consumable outside of battle for now.");
        }
    }

    public void Use(Combatant target)
    {
        if (BaseData != null)
        {
            BaseData.Use(target, Tier);
        }
    }
}
