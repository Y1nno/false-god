using System.Collections.Generic;
using UnityEngine;

public abstract class CombatAction
{
    public int ActionID { get; protected set; } = -1;
    public string ActionName { get; set; } = "Unnamed Action";
    public int ManaCost { get; protected set; } = 0;
    public int Priority { get; set; } = 0;
    public static readonly int k_attackCalculationDivisor = 5;

    private CombatManager _cbtm = RunManager.Instance.GetService<CombatManager>();

    public TargetingType TargetType { get; protected set; } = TargetingType.SingleEnemy;
    public AttackType ActionType { get; protected set; } = AttackType.Physical;

    public abstract void Execute(Combatant user, Combatant target = null);
    public virtual bool CanUse(Combatant user)
    {
        return user.CanAffordMana(ManaCost);
    }

    public bool NeedsTarget()
    {
        return TargetType != TargetingType.Self && TargetType != TargetingType.All;
    }

    public List<Combatant> GetAvailableTargets(Combatant user, TargetingType targetingType)
    {
        List<Combatant> targets = new List<Combatant>();
        List<Combatant> possibleEnemies = _cbtm.CurrentBattle.GetEnemies();
        PlayerCombatManager pcm = _cbtm.CurrentBattle.Pcm;
        bool isUserPlayer = user == pcm;

        switch (targetingType)
        {
            case TargetingType.Self:
                targets.Add(user);
                break;
            case TargetingType.SingleEnemy:
                if (isUserPlayer)
                {
                    targets.AddRange(possibleEnemies);
                }
                else
                {
                    targets.Add(pcm);
                }
                break;
            case TargetingType.AllEnemies:
                if (isUserPlayer)
                {
                    targets.AddRange(possibleEnemies);
                }
                else
                {
                    targets.Add(pcm);
                }
                break;
            case TargetingType.SingleEnemyAlly:
                if (isUserPlayer)
                {
                    targets.Add(pcm);
                }
                else
                {
                    targets.AddRange(possibleEnemies);
                }
                break;
            case TargetingType.AllEnemyAlly:
                if (isUserPlayer)
                {
                    targets.Add(pcm);
                }
                else
                {
                    targets.AddRange(possibleEnemies);
                }
                break;
            case TargetingType.All:
                if (isUserPlayer)
                {
                    targets.AddRange(possibleEnemies);
                    targets.Add(pcm);
                }
                else
                {
                    targets.Add(pcm);
                    targets.AddRange(possibleEnemies);
                }
                break;
            default:
                break;
        }
        return targets;
    }

    protected virtual int CalculateDamageBasedOnStats(int baseDamage, Combatant user)
    {
        int finalDamage = baseDamage + user.GetBonusDamage(); // Add flat flat bonus damage
        if (ActionType == AttackType.Physical)
        {
            finalDamage *= (user.GetSecondaryStat(SecondaryStat.PHATK) + DiceRoller.Instance.RollD20())/k_attackCalculationDivisor;
        }
        else if (ActionType == AttackType.Special)
        {
            finalDamage *= (user.GetSecondaryStat(SecondaryStat.SPATK) + DiceRoller.Instance.RollD20())/k_attackCalculationDivisor;
        }
        return finalDamage;
    }

    protected virtual int CalculateCriticalHit(int baseDamage, Combatant user)
    {
        int finalDamage = baseDamage;

            if (Random.Range(0f, 100f) < user.GetCritChance())
            {
                finalDamage *= 2; // Standard 2x Crit
                TextOutputter.Instance.OutputText("CRITICAL HIT!");
            }

        return finalDamage;
    }

    public virtual int CalculateDamageFromBase(int baseDamage, Combatant user)
    {
        int finalDamage = CalculateDamageBasedOnStats(baseDamage, user);
        finalDamage = CalculateCriticalHit(finalDamage, user);
        return finalDamage;
    }
}

public enum TargetingType
{
    Self,
    SingleEnemy,
    AllEnemies,
    SingleEnemyAlly,
    AllEnemyAlly,
    All,
}

public enum AttackType
{
    Physical,
    Special,
}
