using UnityEngine;
using System.Collections.Generic;

public class JudgementRite : Rite, IObserver
{
    private const int k_EncountersToTrigger = 10;
    private int _encounterCount = 0;
    private EncounterManager _encounterManager;

    // Track accumulated buffs for Description and Unequip
    private float _accDamageMult = 0f;
    private int _accMaxHP = 0;
    private float _accCrit = 0f;
    public float CritChance => _accCrit;
    private Dictionary<Stat, int> _accStats = new Dictionary<Stat, int>
    {
        { Stat.STR, 0 }, { Stat.DEX, 0 }, { Stat.INT, 0 }
    };

    public override string Description
    {
        get
        {
            if (_accDamageMult == 0 && _accMaxHP == 0 && _accCrit == 0 && SumStats() == 0)
            {
                return "Judgement: Grants a permanent buff every 10 encounters.";
            }

            string desc = "Judgement Buffs:\n";
            if (_accDamageMult > 0) desc += $"- Dmg: +{_accDamageMult * 100:0}%\n";
            if (_accMaxHP > 0) desc += $"- Max HP: +{_accMaxHP}\n";
            if (_accCrit > 0) desc += $"- Crit: +{_accCrit}%\n";
            foreach (var kvp in _accStats)
            {
                if (kvp.Value > 0) desc += $"- {kvp.Key}: +{kvp.Value}\n";
            }
            return desc;
        }
    }

    private int SumStats()
    {
        int sum = 0;
        foreach (var v in _accStats.Values) sum += v;
        return sum;
    }

    public JudgementRite() : base("Judgement", 5, RiteType.Judgement) // ID: Judgement, Cost: 5
    {
        RiteType = RiteType.Judgement;
    }

    public override void OnEquip(PlayerManager player)
    {
        _encounterManager = RunManager.Instance.GetService<EncounterManager>();
        _encounterManager.AttachObserver(this);
        _encounterCount = 0;

        ApplyRandomBuff(player);
        TextOutputter.Instance.OutputText("Judgement has begun! A boon is granted.");
    }

    public override void OnUnequip(PlayerManager player)
    {
        if (_encounterManager != null)
        {
            _encounterManager.DetachObserver(this);
        }

        // Revert buffs
        player.DamageMultiplier -= _accDamageMult;
        player.Health.IncreaseBaseMax(-_accMaxHP);
        // Note: We don't remove current HP, just Max.

        _accCrit = 0f;

        foreach (var kvp in _accStats)
        {
             if (kvp.Value > 0)
             {
                 int current = player.PlayerStats.GetStat(kvp.Key);
                 player.PlayerStats.SetStat(kvp.Key, current - kvp.Value);
             }
        }

        TextOutputter.Instance.OutputText("Judgement faded. All boons lost.");
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EncounterResolve)
        {
            _encounterCount++;

            if (_encounterCount >= k_EncountersToTrigger)
            {
                _encounterCount = 0;
                PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
                ApplyRandomBuff(pm);
            }
        }
    }

    private void ApplyRandomBuff(PlayerManager pm)
    {
        int roll = Random.Range(0, 4);
        switch (roll)
        {
            case 0: // +5% Dmg
                pm.DamageMultiplier += 0.05f;
                _accDamageMult += 0.05f;
                TextOutputter.Instance.OutputText("Judgement: Your strength grows! (+5% Damage)");
                break;
            case 1: // +10 Max HP & Heal
                pm.Health.IncreaseBaseMax(10);
                pm.Health.Increase(10);
                _accMaxHP += 10;
                TextOutputter.Instance.OutputText("Judgement: Your vitality overflows! (+10 Max HP)");
                break;
            case 2: // +2 Random Stat
                Stat[] stats = { Stat.STR, Stat.DEX, Stat.INT };
                Stat randomStat = stats[Random.Range(0, stats.Length)];
                int currentStat = pm.PlayerStats.GetStat(randomStat);
                pm.PlayerStats.SetStat(randomStat, currentStat + 2);
                _accStats[randomStat] += 2;
                 TextOutputter.Instance.OutputText($"Judgement: You feel sharper! (+2 {randomStat})");
                break;
            case 3: // +2% Crit
                _accCrit += 2.0f;
                TextOutputter.Instance.OutputText("Judgement: Your aim strikes true! (+2% Crit Chance)");
                break;
        }
    }
}
