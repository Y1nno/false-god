using UnityEngine;

public class BloodDrainAction : CombatAction
{
    private int _baseDamage = 15;

    public BloodDrainAction()
    {
        ActionID = 103;
        ActionName = "Blood Drain";
        ManaCost = 10;
        ActionType = AttackType.Special;
        TargetType = TargetingType.SingleEnemy;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null) return;

        if (!user.TryUseMana(ManaCost)) return;

        TextOutputter.Instance.OutputText($"{user.GetName()} uses {ActionName} on {target.GetName()}!");
        int damage = CalculateDamageFromBase(_baseDamage, user);
        int damageDealt = target.GetAttacked(damage, ActionType, user);

        if (damageDealt > 0)
        {
            int healAmount = Mathf.RoundToInt(damageDealt * 0.5f);
            user.Heal(healAmount);
            TextOutputter.Instance.OutputText($"{user.GetName()} restored {healAmount} HP from the feast of blood!");
        }
    }
}
