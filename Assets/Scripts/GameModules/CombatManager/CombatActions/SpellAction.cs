using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpellAction : CombatAction
{
    private SpellSO _data;

    public SpellAction(SpellSO data)
    {
        _data = data;
        ActionName = data.actionName;
        ManaCost = data.cost;
        TargetType = data.Targeting;
        ActionType = AttackType.Special; // Spells scale with Special Attack as requested
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null && NeedsTarget()) return;

        if (!user.TryUseMana(ManaCost))
        {
            TextOutputter.Instance.OutputText($"{user.GetName()} fails to cast {_data.actionName} (Not enough Mana).");
            return;
        }

        TextOutputter.Instance.OutputText($"{user.GetName()} casts {_data.actionName}!");

        List<Combatant> targets = new List<Combatant>();
        if (TargetType == TargetingType.AllEnemies || TargetType == TargetingType.All)
        {
            targets = GetAvailableTargets(user, TargetType);
        }
        else if (target != null)
        {
            targets.Add(target);
        }

        foreach (var t in targets)
        {
            int hits = Random.Range(_data.MinHits, _data.MaxHits + 1);
            for (int i = 0; i < hits; i++)
            {
                if (!t.IsAlive()) break;

                int damage = CalculateDamageFromBase(_data.power, user);
                int damageDealt = t.GetAttacked(damage, ActionType, user);

                // Handle Special Effects
                if (damageDealt > 0 && _data.Effect != SpellEffect.None)
                {
                    ApplyEffect(user, t, damageDealt);
                }
            }
        }
    }

    private void ApplyEffect(Combatant user, Combatant target, int damageDealt)
    {
        if (Random.Range(0, 100) >= _data.EffectChance) return;

        switch (_data.Effect)
        {
            case SpellEffect.Burn:
                target.ApplyAilment(AilmentType.Burn, _data.Duration);
                break;
            case SpellEffect.Poison:
                target.ApplyAilment(AilmentType.Poison, _data.Duration);
                break;
            case SpellEffect.Freeze:
                target.ApplyAilment(AilmentType.Frozen, _data.Duration);
                break;
            case SpellEffect.Weaken:
                target.ApplyAilment(AilmentType.AtkDebuff, _data.Duration); // Weaken = AtkDebuff
                break;
            case SpellEffect.Lifesteal:
                int heal = Mathf.RoundToInt(damageDealt * _data.HealingPercent);
                user.Heal(heal);
                TextOutputter.Instance.OutputText($"{user.GetName()} drains {heal} HP!");
                break;
            case SpellEffect.Cleanse:
                user.ActiveAilments.Clear();
                TextOutputter.Instance.OutputText($"{user.GetName()} is cleansed of all ailments.");
                break;
            case SpellEffect.Invulnerable:
                // target.ApplyAilment(AilmentType.Invulnerable, _data.Duration); // If Invulnerable enum exists
                break;
            case SpellEffect.StatBuff:
                // Buff logic (e.g., +30% def) - could use a temporary ActiveOverTimeEffect
                break;
        }
    }
}
