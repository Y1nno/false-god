using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    public BossSO BossData { get; private set; }
    public Boss(BossSO data, int level = 1) : base(data.BossName, null, data.BaseHP, data.BaseMana, new List<int> { 01, data.SpecialActionID }, data.XP, true)
    {
        BossData = data;
        Level = level;
        ApplyStats();
    }

    private void ApplyStats()
    {
        // Monster Stats multiplier formula:
        // HP = BaseHP * (1 + (L - 1) * 0.25)
        // Atk = BaseAtk * (1 + (L - 1) * 0.18)
        // SpAtk = BaseSpAtk * (1 + (L - 1) * 0.18)
        // Def = BaseDef * (1 + (L - 1) * 0.12)
        // SpDef = BaseSpDef * (1 + (L - 1) * 0.12)
        // Speed = BaseSpeed + (L - 1) * 0.4 (depth/floor)
        // XP = BaseXP + L * 0.25 (Wait, the image says L x 0.25, maybe it's level based bonus)

        float l_factor = Level - 1;
        
        int maxHp = Mathf.RoundToInt(BossData.BaseHP * (1 + l_factor * 0.25f));
        Health = new Resource(maxHp);

        int maxMana = BossData.BaseMana; // Mana doesn't seem to scale in formula
        Mana = new Resource(maxMana);

        Dictionary<Stat, int> stats = new Dictionary<Stat, int>
        {
            { Stat.STR, Mathf.RoundToInt(BossData.STR * (1 + l_factor * 0.18f)) },
            { Stat.DEX, BossData.DEX }, // Dex doesn't seem to scale in formula
            { Stat.INT, Mathf.RoundToInt(BossData.INT * (1 + l_factor * 0.18f)) },
            { Stat.SPD, Mathf.RoundToInt(BossData.SPD + l_factor * 0.4f) }
        };
        Stats = new EnemyStatBox(stats);

        // TODO: Handle Def/SpDef scaling within EnemyStatBox or Enemy overrides if necessary
        // Currently EnemyStatBox seems to just store base stats.
    }

    protected override int TakeDamage(int amount)
    {
        int actualDamage = base.TakeDamage(amount);

        // Gorvath Passive: Heal 50% when HP < 20%
        if (BossData.BossName == "Gorvath" && Health.CurrentValue > 0 && (float)Health.CurrentValue / Health.MaxValue < 0.2f)
        {
            // Simple flag to prevent infinite healing if not specified otherwise
            // For now, let's just heal once or every time it's hit while low
            int healAmount = Health.MaxValue / 2;
            Heal(healAmount);
            TextOutputter.Instance.OutputText($"{BossData.BossName} uses its passive to regenerate {healAmount} HP!");
        }

        return actualDamage;
    }

    public override int GetAttacked(int damage = 0, AttackType attackType = AttackType.Physical, Combatant attacker = null)
    {
        int result = base.GetAttacked(damage, attackType, attacker);

        // Molech Passive: 30% chance to counter on physical attack
        if (BossData.BossName == "Molech" && attackType == AttackType.Physical && Random.value <= 0.3f && IsAlive())
        {
            TextOutputter.Instance.OutputText($"{BossData.BossName} counters!");
            // Execute a basic attack back at the attacker
            CombatAction counterAction = ActionFactory.CreateActionByID(01);
            counterAction.Execute(this, attacker);
        }

        return result;
    }

    public override void ExecuteAction()
    {
        base.ExecuteAction();

        // Vlad Passive: Vampirism (Steal 5% Max HP/MP)
        // This is a bit vague - maybe on every action? 
        if (BossData.BossName == "Vlad" && IsAlive() && CurrentAction != null && CurrentAction.ActionID != 000)
        {
            PlayerCombatManager pcm = RunManager.Instance.GetService<CombatManager>().CurrentBattle.Pcm;
            int hpSteal = Mathf.RoundToInt(pcm.GetHealth().MaxValue * 0.05f);
            int mpSteal = Mathf.RoundToInt(pcm.GetMana().MaxValue * 0.05f);
            
            pcm.GetHealth().Decrease(hpSteal);
            pcm.GetMana().Decrease(mpSteal);
            Heal(hpSteal);
            RestoreMana(mpSteal);
            
            TextOutputter.Instance.OutputText($"{BossData.BossName} steals {hpSteal} HP and {mpSteal} Mana with Vampirism!");
        }
    }

    // Phases (Future logic)
    /*
    private void CheckPhaseTransitions()
    {
        // Placeholder for future phase logic
    }
    */
}
