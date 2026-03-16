using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        float relicMultiplier = (user is PlayerCombatManager) && relicm != null ? relicm.GetDamageMultiplier() : 1.0f;
        int relicFlatBonus = (user is PlayerCombatManager) && relicm != null ? relicm.GetFlatDamageBonus() : 0;
        int hunterTrophyBonus = (user is PlayerCombatManager) && relicm != null ? relicm.GetHunterTrophyFlatBonus() : 0;

        int finalDamage = Mathf.RoundToInt((baseDamage + user.GetBonusDamage() + relicFlatBonus + hunterTrophyBonus) * relicMultiplier);
        
        // Aura of the Fallen (Fallen Saint Passive): Reduces enemy damage by 20%
        if (user is PlayerCombatManager)
        {
            CombatManager cm = RunManager.Instance.GetService<CombatManager>();
            if (cm != null && cm.CurrentBattle != null)
            {
                if (cm.CurrentBattle.GetEnemies().Any(e => e.GetName() == "Fallen Saint" && e.IsAlive()))
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * 0.8f);
                    // We don't log here to avoid spamming every hit, but the effect is applied.
                }
            }
        }

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
            RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
            float multiplier = (user is PlayerCombatManager) && relicm != null ? relicm.GetCritDamageMultiplier() : 2.0f;
            
            finalDamage = Mathf.RoundToInt(finalDamage * multiplier);
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
