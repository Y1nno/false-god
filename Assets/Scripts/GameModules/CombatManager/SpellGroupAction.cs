using UnityEngine;

public class SpellGroupAction : CombatAction
{
    public SpellGroupAction()
    {
        ActionName = "Use Spell";
        ActionID = 888;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        // This action is just a menu trigger, it shouldn't be executed directly.
        Debug.LogWarning("SpellGroupAction executed directly. This should not happen.");
    }
}
