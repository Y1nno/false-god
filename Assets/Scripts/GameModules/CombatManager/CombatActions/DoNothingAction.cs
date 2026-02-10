using UnityEngine;

public class DoNothingAction : CombatAction
{
    public DoNothingAction()
    {
        ActionID = 000;
        ActionName = "Do Nothing";
        ManaCost = 0;
        TargetType = TargetingType.Self;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        //Debug.Log($"{user} chose to do nothing.");
    }

    public override bool CanUse(Combatant user)
    {
        return true; // Always can use "Do Nothing"
    }
}
