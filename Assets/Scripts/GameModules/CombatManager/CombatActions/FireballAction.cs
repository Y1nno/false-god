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

        TextOutputter.Instance.OutputText($"{user.GetName()} cast {ActionName} on {target.GetName()}!");
        int damage = _baseDamage;
        damage = CalculateDamageFromBase(damage, user);
        int damageDealt = target.GetAttacked(damage, ActionType, user);

        if (user is PlayerCombatManager pcm && damageDealt > 0)
        {
            EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
            if (eqm != null && eqm.HasTrait(EquipmentTrait.SoulSteal))
            {
                int stealAmount = Mathf.Max(1, (int)(damageDealt * 0.2f));
                pcm.GetMana().Increase(stealAmount);
                TextOutputter.Instance.OutputText($"{user.GetName()} restored {stealAmount} Mana via SoulSteal!");
            }
        }
    }
}
