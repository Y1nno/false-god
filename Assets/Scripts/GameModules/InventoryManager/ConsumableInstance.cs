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
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        EncounterManager em = RunManager.Instance.GetService<EncounterManager>();

        if (em != null && em.GetCurrentEncounter() != null)
        {
            // We have an encounter active. Use the persistent PCM from CombatManager.
            if (cm != null && cm.Pcm != null)
            {
                Use(cm.Pcm);
                return;
            }
        }
        
        TextOutputter.Instance.OutputText("Cannot use consumable outside of an encounter.");
    }

    public void Use(Combatant target)
    {
        if (BaseData != null)
        {
            BaseData.Use(target, Tier);
        }
    }
}
