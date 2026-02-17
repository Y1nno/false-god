using UnityEngine;
using System.Collections.Generic;

public class JudgementRite : Rite, IObserver
{
    private const int k_EncountersToTrigger = 10;
    private int _encounterCount = 0;
    private EncounterManager _encounterManager;

    public override string Description => "Judgement: Grants a permanent buff every 10 encounters.";

    public JudgementRite() : base("Judgement", 5) // ID: Judgement, Cost: 5
    {
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
                TextOutputter.Instance.OutputText("Judgement: Your strength grows! (+5% Damage)");
                break;
            case 1: // +10 Max HP & Heal
                pm.Health.IncreaseBaseMax(10);
                pm.Health.Increase(10);
                TextOutputter.Instance.OutputText("Judgement: Your vitality overflows! (+10 Max HP)");
                break;
            case 2: // +2 Random Stat
                Stat randomStat = (Stat)Random.Range(0, 3); // STR=0, INT=1, DEX=2
                pm.PlayerStats.SetStat(randomStat, pm.PlayerStats.GetStat(randomStat) + 2);
                 TextOutputter.Instance.OutputText($"Judgement: You feel sharper! (+2 {randomStat})");
                break;
            case 3: // +2% Crit
                pm.CritChance += 2.0f;
                TextOutputter.Instance.OutputText("Judgement: Your aim strikes true! (+2% Crit Chance)");
                break;
        }
    }
}
