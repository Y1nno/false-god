using UnityEngine;
using System.Linq;

public class PlayerAttackAction : CombatAction
{
    private int _baseDamage = 10;
    public PlayerAttackAction()
    {
        ActionID = 1;
        ActionName = "Attack";
        ManaCost = 0;
        ActionType = AttackType.Physical;
        TargetType = TargetingType.SingleEnemy;
    }

    public override bool CanUse(Combatant user)
    {
        if (!base.CanUse(user)) return false;

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null && eqm.HasMoveFirstAvailable(out EquipmentSO moveFirstItem, out int traitIndex))
        {
            if (moveFirstItem.Traits[traitIndex].CurrentCooldown > 0)
            {
                return false; // Attack blocked while Bow is reloading
            }
        }

        return true;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        if (target == null)
        {
            GetAvailableTargets(user, TargetType);
            return;
        }

        int damage = _baseDamage;

        TextOutputter.Instance.OutputText($"{user.GetName()} used {ActionName} on {target.GetName()}!");
        damage = CalculateDamageFromBase(damage, user);
        int damageDealt = target.GetAttacked(damage, ActionType, user);

        // Relic: Splash Damage
        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        if (relicm != null && user is PlayerCombatManager)
        {
            float splashPercent = relicm.GetSplashDamagePercentage();
            if (splashPercent > 0)
            {
                int splashDamage = Mathf.RoundToInt(damageDealt * splashPercent);
                if (splashDamage > 0)
                {
                    CombatManager cm = RunManager.Instance.GetService<CombatManager>();
                    if (cm != null && cm.CurrentBattle != null)
                    {
                        var otherEnemies = cm.CurrentBattle.GetEnemies()
                            .Where(e => e != target && e.IsAlive())
                            .ToList();
                        
                        if (otherEnemies.Count > 0)
                        {
                            TextOutputter.Instance.OutputText($"Splash damage dealt {splashDamage} to other enemies!");
                            foreach (var enemy in otherEnemies)
                            {
                                enemy.GetAttacked(splashDamage, ActionType, user);
                            }
                        }
                    }
                }
            }
        }

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        
        if (user is PlayerCombatManager pcmUser)
        {
            eqm?.DegradeEquippedWeapons(); // Drain durability on hit

            if (eqm != null && eqm.HasTraitAvailable(EquipmentTrait.DoubleStrike, out EquipmentSO item, out int traitIndex))
            {
                float procChance = item.Traits[traitIndex].Value;
                if (Random.Range(0f, 100f) < procChance)
                {
                    if (target.IsAlive())
                    {
                        TextOutputter.Instance.OutputText($"{user.GetName()} strikes again!");
                        damageDealt += target.GetAttacked(damage, ActionType, user);
                    }
                }
            }
            else if (eqm != null && eqm.HasTrait(EquipmentTrait.ComboStrike))
            {
                float roll = Random.Range(0f, 100f);
                int totalStrikes = (roll < 50f) ? 2 : (roll < 80f) ? 3 : 4;
                
                for (int i = 1; i < totalStrikes; i++)
                {
                    if (target.IsAlive())
                    {
                        TextOutputter.Instance.OutputText($"{user.GetName()} combo strikes again!");
                        damageDealt += target.GetAttacked(damage, ActionType, user);
                    }
                }
            }

            if (damageDealt > 0 && eqm != null && eqm.HasTrait(EquipmentTrait.SoulSteal))
            {
                int stealAmount = Mathf.Max(1, (int)(damageDealt * 0.2f));
                pcmUser.GetMana().Increase(stealAmount);
                TextOutputter.Instance.OutputText($"{user.GetName()} restored {stealAmount} Mana via SoulSteal!");
            }
        }
    }
}
