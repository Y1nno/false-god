using UnityEngine;

public class SwitchWeaponAction : CombatAction
{
    public SwitchWeaponAction()
    {
        ActionID = 999;
        ActionName = "Switch Weapon";
        ManaCost = 0;
        ActionType = AttackType.Physical;
        TargetType = TargetingType.Self;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        TextOutputter.Instance.OutputText($"{user.GetName()} spent their turn switching equipment.");
    }
}
