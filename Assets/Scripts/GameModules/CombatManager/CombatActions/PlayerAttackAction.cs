using UnityEngine;

public class PlayerAttackAction : CombatAction
{
    public PlayerAttackAction()
    {
        ActionID = 1;
        ActionName = "Attack";
        ManaCost = 0;
        TargetType = TargetingType.SingleEnemy;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null)
        {
            GetAvailableTargets(user, TargetType);
            return;
        }

        int damage = 10; // Example fixed damage
        target.TakeDamage(damage);
    }
}
