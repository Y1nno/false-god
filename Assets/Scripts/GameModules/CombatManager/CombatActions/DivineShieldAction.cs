using UnityEngine;

public class DivineShieldAction : CombatAction
{
    public DivineShieldAction()
    {
        ActionID = 104;
        ActionName = "Divine Shield";
        ManaCost = 5;
        TargetType = TargetingType.Self;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (!user.TryUseMana(ManaCost)) return;

        TextOutputter.Instance.OutputText($"{user.GetName()} casts {ActionName}!");
        user.ApplyAilment(AilmentType.Shielded, 3);
    }
}
