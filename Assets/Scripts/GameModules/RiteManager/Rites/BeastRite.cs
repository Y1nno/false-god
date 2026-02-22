using UnityEngine;
using System;

public class BeastRite : Rite, IObserver
{
    private const int k_MaxStacks = 20;
    private const int k_DamagePerStack = 1;

    // We track stacks locally to ensure we don't exceed max,
    // even though the effect is applied directly to the player's BonusDamage.
    private int _currentStacks = 0;

    // We need to keep a reference to the observed subject (CombatManager) to detach later.
    private CombatManager _combatManager;

    public override string Description => $"Beast (+{_currentStacks} Dmg)";

    public BeastRite() : base("Beast", 5, RiteType.Beast) // ID: Beast, Cost: 5
    {
        RiteType = RiteType.Beast;
    }

    public override void OnEquip(PlayerManager player)
    {
        _currentStacks = 0;

        _combatManager = RunManager.Instance.GetService<CombatManager>();

        _combatManager.AttachObserver(this);

        TextOutputter.Instance.OutputText("Beast Rite: Listening for bloodshed...");
    }

    public override void OnUnequip(PlayerManager player)
    {
        player.BonusDamage -= _currentStacks * k_DamagePerStack;

        if (_combatManager != null)
        {
            _combatManager.DetachObserver(this);
            _combatManager = null;
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {

        if (eventType == EventType.EnemyDefeated)
        {
            AddStack();
        }
    }

    private void AddStack()
    {
        if (_currentStacks < k_MaxStacks)
        {
            _currentStacks++;
            RunManager.Instance.GetService<PlayerManager>().BonusDamage += k_DamagePerStack;
            TextOutputter.Instance.OutputText($"Beast Rite hungers! (+{_currentStacks} Dmg)");
        }
    }
}
