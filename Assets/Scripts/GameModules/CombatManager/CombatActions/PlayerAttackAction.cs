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
             PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
             damage += pm.BonusDamage;
             damage = Mathf.RoundToInt(damage * pm.DamageMultiplier);

             // Critical Hit Check
             if (Random.Range(0f, 100f) < pm.CritChance)
             {
                 damage *= 2; // Standard 2x Crit
                 TextOutputter.Instance.OutputText("CRITICAL HIT!");
             }
        }

        target.TakeDamage(damage);
    }
}
