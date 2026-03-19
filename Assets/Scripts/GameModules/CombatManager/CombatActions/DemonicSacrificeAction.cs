using UnityEngine;

public class DemonicSacrificeAction : CombatAction
{
    public DemonicSacrificeAction()
    {
        ActionID = 102;
        ActionName = "Demonic Sacrifice";
        ManaCost = 10;
        TargetType = TargetingType.Self;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (!user.TryUseMana(ManaCost)) return;

        TextOutputter.Instance.OutputText($"{user.GetName()} performs a {ActionName}!");

        // Deals -20% hp dmg to itself
        int selfDamage = Mathf.RoundToInt(user.GetHealth().MaxValue * 0.2f);
        user.TakeConsumableDamage(selfDamage);
        TextOutputter.Instance.OutputText($"{user.GetName()} sacrificed {selfDamage} HP!");

        // Gains +40% atk
        user.ApplyAilment(AilmentType.AtkBonus, 3);
    }
}
