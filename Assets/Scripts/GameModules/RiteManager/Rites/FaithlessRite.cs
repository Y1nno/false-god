using UnityEngine;
using System.Collections.Generic;

public class FaithlessRite : Rite, IObserver
{
    private const int k_StatBonus = 5;
    private const int k_EncountersPerBonus = 30;

    private int _encounterCount = 0;
    private int _totalStatsGained = 0; // Track total added to each stat to remove on unequip

    // We need to observe EncounterManager to track progress
    private EncounterManager _encounterManager;

    public override string Description => $"Faithless: +{_totalStatsGained} All Attributes. Next bonus in {k_EncountersPerBonus - _encounterCount} encounters.";

    public FaithlessRite() : base("Faithless", 10, RiteType.Faithless)
    {
        RiteType = RiteType.Faithless;
    }

    public override void OnEquip(PlayerManager player)
    {
        _encounterCount = 0;
        _totalStatsGained = 0;

        // Apply initial bonus
        ApplyStatBonus(player);

        // Subscribe to EncounterManager
        _encounterManager = RunManager.Instance.GetService<EncounterManager>();
        if (_encounterManager != null)
        {
            _encounterManager.AttachObserver(this);
        }
    }

    public override void OnUnequip(PlayerManager player)
    {
        // Remove all gained stats
        RemoveStatBonus(player);

        // Unsubscribe
        if (_encounterManager != null)
        {
            _encounterManager.DetachObserver(this);
            _encounterManager = null;
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EncounterResolve)
        {
            _encounterCount++;
            if (_encounterCount >= k_EncountersPerBonus)
            {
                _encounterCount = 0;
                PlayerManager player = RunManager.Instance.GetService<PlayerManager>();
                if (player != null)
                {
                    ApplyStatBonus(player);
                    TextOutputter.Instance.OutputText($"Faithless Rite: Your conviction deepens. (+{k_StatBonus} All Attributes)");
                }
            }
        }
    }

    private void ApplyStatBonus(PlayerManager player)
    {
        player.IncreaseStat(Stat.STR, k_StatBonus);
        player.IncreaseStat(Stat.DEX, k_StatBonus);
        player.IncreaseStat(Stat.INT, k_StatBonus);
        // player.IncreaseStat(Stat.LCK, k_StatBonus); // LCK removed
        
        _totalStatsGained += k_StatBonus;
    }

    private void RemoveStatBonus(PlayerManager player)
    {
        player.DecreaseStat(Stat.STR, _totalStatsGained);
        player.DecreaseStat(Stat.DEX, _totalStatsGained);
        player.DecreaseStat(Stat.INT, _totalStatsGained);
        // player.DecreaseStat(Stat.LCK, _totalStatsGained);
    }
}
