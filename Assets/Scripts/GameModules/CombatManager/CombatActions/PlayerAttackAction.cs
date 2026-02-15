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

        int damage = user.GetStat(Stat.STR);
        if (user is PlayerCombatManager)
        {
             damage += RunManager.Instance.GetService<PlayerManager>().BonusDamage;
        }

        target.TakeDamage(damage);
    }
}
