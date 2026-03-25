using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BashAction : CombatAction
{
    public BashAction()
    {
        ActionID = 201;
        ActionName = "Bash";
        ManaCost = 5;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null) return;
        TextOutputter.Instance.OutputText($"{user.GetName()} used {ActionName} on {target.GetName()}!");
        
        int damage = 6;
        damage = CalculateDamageFromBase(damage, user);
        TextOutputter.Instance.OutputText($"{user.GetName()}'s attack roll: {damage}.");
        target.GetAttacked(damage, ActionType, user);

        target.TryApplyAilment(AilmentType.Stun, 1, 50f);
    }
}

public class StealGoldAction : CombatAction
{
    public StealGoldAction()
    {
        ActionID = 202;
        ActionName = "Steal Gold";
        ManaCost = 5;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null) return;
        TextOutputter.Instance.OutputText($"{user.GetName()} used {ActionName} on {target.GetName()}!");

        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (em != null)
        {
            int currentGold = em.GetCurrentGold();
            int stolen = Mathf.RoundToInt(currentGold * 0.05f);
            if (stolen > 0)
            {
                em.LoseGold(stolen);
                TextOutputter.Instance.OutputText($"{user.GetName()} stole {stolen} gold!");
            }
        }
    }
}

public class CurseAction : CombatAction
{
    public CurseAction()
    {
        ActionID = 203;
        ActionName = "Curse";
        ManaCost = 5;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null) return;
        TextOutputter.Instance.OutputText($"{user.GetName()} cast {ActionName} on {target.GetName()}!");

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null)
        {
            // Placeholder: Disables random equipment for 2 turns. 
            // Implementation depends on EquipmentManager having a 'DisableRandomEquipment' method.
            // For now, let's just apply a 'Cursed' ailment that reduces stats significantly.
            target.ApplyAilment(AilmentType.AtkDebuff, 2);
            target.ApplyAilment(AilmentType.DefDebuff, 2);
            TextOutputter.Instance.OutputText($"{target.GetName()} is cursed and feels weakened!");
        }
    }
}

public class DevourAction : CombatAction
{
    public DevourAction()
    {
        ActionID = 204;
        ActionName = "Devour";
        ManaCost = 0;
    }

    private int _turnsWaiting = 0;

    public override void Execute(Combatant user, Combatant target = null)
    {
        _turnsWaiting++;
        if (_turnsWaiting < 3)
        {
            TextOutputter.Instance.OutputText($"{user.GetName()} is preparing to devour {target.GetName()}... ({_turnsWaiting}/3)");
            return;
        }

        if (target == null) return;
        TextOutputter.Instance.OutputText($"{user.GetName()} DEVOURS {target.GetName()}!");
        int damage = Mathf.RoundToInt(target.GetHealth().MaxValue * 0.5f);
        TextOutputter.Instance.OutputText($"{user.GetName()}'s devour damage: {damage}.");
        target.GetAttacked(damage, AttackType.Physical, user);
        _turnsWaiting = 0;
    }
}

public class EscapeAction : CombatAction
{
    public EscapeAction()
    {
        ActionID = 205;
        ActionName = "Escape";
        ManaCost = 0;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        TextOutputter.Instance.OutputText($"{user.GetName()} runs away from the battle!");
        user.Die(); // Simple way to remove from battle without reward
    }
}

public class CharmAction : CombatAction
{
    public CharmAction()
    {
        ActionID = 206;
        ActionName = "Charm";
        ManaCost = 5;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null) return;
        TextOutputter.Instance.OutputText($"{user.GetName()} cast {ActionName} on {target.GetName()}!");

        target.TryApplyAilment(AilmentType.Charmed, 1, 50f);
    }
}
