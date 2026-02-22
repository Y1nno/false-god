using UnityEngine;

public class ChaliceRite : Rite, IObserver
{
    private const int k_MaxStacks = 50;
    private int _currentStacks = 0;

    public override string Description => $"Gain +1 Permanent Max HP per kill. (Current: {_currentStacks}/{k_MaxStacks})";

    public ChaliceRite() : base("Chalice", 5)
    {
        RiteType = RiteType.Chalice;
    }

    public override void OnEquip(PlayerManager player)
    {
        _currentStacks = 0; // Reset stacks on new equip
        // Subscribe to CombatManager to listen for enemy deaths
        CombatManager combatManager = RunManager.Instance.GetService<CombatManager>();
        if (combatManager != null)
        {
            combatManager.AttachObserver(this);
        }
    }

    public override void OnUnequip(PlayerManager player)
    {
        // Remove the accumulated Max HP
        player.Health.IncreaseBaseMax(-_currentStacks);

        // Unsubscribe to stop tracking
        CombatManager combatManager = RunManager.Instance.GetService<CombatManager>();
        if (combatManager != null)
        {
            combatManager.DetachObserver(this);
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EnemyDefeated)
        {
            if (_currentStacks < k_MaxStacks)
            {
                _currentStacks++;
                PlayerManager player = RunManager.Instance.GetService<PlayerManager>();
                if (player != null)
                {
                    player.Health.IncreaseBaseMax(1);
                    TextOutputter.Instance.OutputText($"Chalice absorbed a soul! Max HP +1 ({_currentStacks}/{k_MaxStacks})");
                }
            }
        }
    }
}
