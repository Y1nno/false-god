using UnityEngine;

public class FireballAction : CombatAction
{
    private int _baseDamage = 10;
    public FireballAction()
    {
        ActionID = 2;
        ActionName = "Fireball";
        ManaCost = 20;
        TargetType = TargetingType.SingleEnemy;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null)
        {
            GetAvailableTargets(user, TargetType);
            return;
        }

        if (!user.TryUseMana(ManaCost))
        {
            Debug.Log($"{user} does not have enough mana to cast {ActionName}.");
            return;
        }

        int damage = _baseDamage;
        damage = CalculateDamageFromBase(damage, user);
        target.GetAttacked(damage, ActionType);
    }
}
