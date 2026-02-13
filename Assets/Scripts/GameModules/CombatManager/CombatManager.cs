using System.Collections.Generic;
using UnityEngine;

public class CombatManager : GameModule, IObserver
{
    public Battle CurrentBattle {get; private set;}
    public override void AttachDefaultObservers(){}

    public void CreateNewBattle(float difficulty)
    {
        CurrentBattle = new Battle(EnemiesByDifficulty(difficulty));
        CurrentBattle.AttachObserver(this);
    }

    public void Start()
    {
        if (CurrentBattle == null)
        {
            Debug.LogWarning("CombatManager: No current battle to start.");
            return;
        }
        CurrentBattle.StartBattle();
    }

    private List<Combatant> EnemiesByDifficulty(float difficulty)
    {
        // Placeholder logic for generating enemies based on difficulty
        List<Combatant> enemies = new List<Combatant>();
        enemies.Add(new Goblin());
        return enemies;
    }

    private void FinishBattle()
    {
        CurrentBattle = null;
        Notify(EventType.BattleEnd);
    }

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.BattleEnd:
                FinishBattle();
                break;
            default:
                break;
        }
    }
}
