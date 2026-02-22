using UnityEngine;

public class PlayerAttackAction : CombatAction
{
    private int _baseDamage = 10;
    public PlayerAttackAction()
    {
        ActionID = 1;
        ActionName = "Attack";
        ManaCost = 0;
        ActionType = AttackType.Physical;
        TargetType = TargetingType.SingleEnemy;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null)
        {
            GetAvailableTargets(user, TargetType);
            return;
        }

        int damage = _baseDamage;
        damage = CalculateDamageFromBase(damage, user);
        target.GetAttacked(damage, ActionType);
    }
}
