using UnityEngine;

public class FireballAction : CombatAction
{
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

        if (!CanUse(user))
        {
            Debug.Log($"{user} does not have enough mana to cast {ActionName}.");
            return;
        }

        user.GetMana().Decrease(ManaCost);

        int damage = 30; // Example fixed damage for Fireball
        target.TakeDamage(damage);
    }
}
