using System.Collections.Generic;
using UnityEngine;

public class CombatManager : GameModule, IObserver
{
    public Battle CurrentBattle {get; private set;}
    public PlayerCombatManager Pcm { get; private set; } = new PlayerCombatManager();

    public override void AttachDefaultObservers()
    {
        RunManager.Instance.GetService<EncounterManager>()?.AttachObserver(this);
    }

    public void CreateNewBattle(float difficulty)
    {
        CurrentBattle = new Battle(EnemiesByDifficulty(difficulty), Pcm);
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
        EncounterManager em = RunManager.Instance.GetService<EncounterManager>();
        if (em != null && em.GetCurrentEncounter() is BossEncounter)
        {
            return new List<Combatant> { CreateBoss() };
        }

        // Placeholder logic for generating enemies based on difficulty
        List<Combatant> enemies = new List<Combatant>();
        enemies.Add(new Goblin());
        return enemies;
    }

    private Boss CreateBoss()
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int floor = dm.CurrentDungeonFloor;
        int room = dm.RoomAtCurrentFloor;

        string bossName = "";
        if (floor == 4 && room == 10)
        {
            bossName = "Fallen Saint";
        }
        else
        {
            string[] earlyBosses = { "Gorvath", "Molech", "Vlad" };
            bossName = earlyBosses[Random.Range(0, earlyBosses.Length)];
        }

        BossSO bossData = Resources.Load<BossSO>($"Bosses/{bossName}");
        if (bossData == null)
        {
            Debug.LogError($"CombatManager: Could not load BossSO for {bossName} at Resources/Bosses/{bossName}");
            return null; // Should probably have a fallback
        }

        return new Boss(bossData, floor); // Simplified Level = floor for now
    }

    private void FinishBattle()
    {
        if (CurrentBattle != null)
        {
            CurrentBattle.Cleanup();
        }
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
            case EventType.EncounterResolve:
                Pcm.ClearEncounterEffects();
                Notify(EventType.EncounterResolve);
                break;
            case EventType.EnemyDefeated:
                Notify(EventType.EnemyDefeated);
                break;
            default:
                break;
        }
    }
}
