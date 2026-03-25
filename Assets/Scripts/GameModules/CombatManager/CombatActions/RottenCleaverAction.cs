using UnityEngine;

public class RottenCleaverAction : CombatAction
{
    private int _baseDamage = 20;

    public RottenCleaverAction()
    {
        ActionID = 101;
        ActionName = "Rotten Cleaver";
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
        target.GetAttacked(damage, ActionType, user);

        // 30% chance to deal poison or bleed
        if (Random.value < 0.5f)
            target.TryApplyAilment(AilmentType.Poison, 1, 30f);
        else
            target.TryApplyAilment(AilmentType.Bleed, 1, 30f);
    }
}
