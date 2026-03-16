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

    private HashSet<string> _defeatedBosses = new HashSet<string>();

    public void InstaKillCurrentBattle()
    {
        if (CurrentBattle == null) return;

        // Collect all enemies currently in the battle
        List<Enemy> enemiesToKill = new List<Enemy>();
        foreach (var combatant in CurrentBattle.combatants)
        {
            if (combatant is Enemy enemy && enemy.IsAlive())
            {
                enemiesToKill.Add(enemy);
            }
        }

        // Kill them all!
        foreach (var enemy in enemiesToKill)
        {
            // Apply massive damage to ensure death
            enemy.TakeConsumableDamage(99999); 
            
            // Explicitly notify observers that THIS specific enemy was defeated
            // This ensures DropManager rolls loot and CombatManager marks it as defeated if it's a boss
            Notify(enemy, EventType.EnemyDefeated);
        }

        TextOutputter.Instance.OutputText("Encounter skipped via instant resolution.");
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
            List<string> earlyBossPool = new List<string> { "Gorvath", "Molech", "Vlad" };
            // Filter out already defeated bosses
            List<string> availableBosses = new List<string>();
            foreach(string b in earlyBossPool)
            {
                if (!_defeatedBosses.Contains(b))
                {
                    availableBosses.Add(b);
                }
            }

            // If we ran out of unique bosses, reset the pool (or fallback)
            if (availableBosses.Count == 0)
            {
                availableBosses = earlyBossPool;
            }

            bossName = availableBosses[Random.Range(0, availableBosses.Count)];
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
                if (subject is Boss boss)
                {
                    _defeatedBosses.Add(boss.BossData.BossName);
                }
                Notify(EventType.EnemyDefeated);
                break;
            default:
                break;
        }
    }
}
