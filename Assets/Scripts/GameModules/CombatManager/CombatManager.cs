// TRIGGER RECOMPILE: 1773950000
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : GameModule, IObserver
{
    public Battle CurrentBattle {get; private set;}
    public PlayerCombatManager Pcm { get; private set; } = new PlayerCombatManager();

    private List<Combatant> _restoredEnemies;

    public override void AttachDefaultObservers()
    {
        RunManager.Instance.GetService<EncounterManager>()?.AttachObserver(this);
    }

    public void CreateNewBattle(float difficulty)
    {
        List<Combatant> enemies = _restoredEnemies ?? EnemiesByDifficulty(difficulty);
        _restoredEnemies = null;

        CurrentBattle = new Battle(enemies, Pcm);
        if (_restoredTurn > 1) CurrentBattle.RestoreTurnCount(_restoredTurn);
        _restoredTurn = 1;
        CurrentBattle.AttachObserver(this);
    }

    public void PrepareRestoredBattle(RunSaveData data)
    {
        _restoredEnemies = new List<Combatant>();
        for (int i = 0; i < data.CurrentEnemyIDs.Count; i++)
        {
            string id = data.CurrentEnemyIDs[i];
            ScriptableObject so = Resources.Load<EnemySO>($"Enemies/{id}");
            if (so == null) so = Resources.Load<BossSO>($"Bosses/{id}");

            if (so != null)
            {
                Enemy enemy = null;
                if (so is BossSO bso) enemy = new Boss(bso, data.CurrentEnemyLevels[i]);
                else if (so is EnemySO eso) enemy = new Enemy(eso, data.CurrentEnemyLevels[i]);

                if (enemy != null && data.EnemyAilments != null && i < data.EnemyAilments.Count)
                {
                    enemy.RestoreAilments(data.EnemyAilments[i]);
                }
                if (enemy != null) _restoredEnemies.Add(enemy);
            }
        }

        Pcm.RestoreAilments(data.PlayerAilments);
        
        // We will set the turn count once the Battle is created in CreateNewBattle
        _restoredTurn = data.CombatTurn;
    }

    private int _restoredTurn = 1;

    public void Start()
    {
        if (CurrentBattle == null)
        {
            Debug.LogWarning("CombatManager: No current battle to start.");
            return;
        }
        Pcm.OnBattleStart();
        CurrentBattle.StartBattle();
    }

    private List<Combatant> EnemiesByDifficulty(float difficulty)
    {
        EncounterManager em = RunManager.Instance.GetService<EncounterManager>();
        if (em != null && em.GetCurrentEncounter() is BossEncounter)
        {
            return new List<Combatant> { CreateBoss() };
        }

        List<Combatant> enemies = new List<Combatant>();
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int floor = dm != null ? dm.CurrentDungeonFloor : 1;

        DepthEncounterTableSO[] tables = Resources.LoadAll<DepthEncounterTableSO>("EncounterTables");
        DepthEncounterTableSO activeTable = null;
        foreach (var t in tables)
        {
            if (floor >= t.MinDepth && floor <= t.MaxDepth)
            {
                activeTable = t;
                break;
            }
        }

        if (activeTable == null)
        {
            Debug.LogWarning($"No Encounter Table found for depth {floor}. Falling back to default Skeleton.");
            var fallbackSo = Resources.Load<EnemySO>("Enemies/Skeleton");
            if (fallbackSo != null) enemies.Add(new Enemy(fallbackSo, floor));
            return enemies;
        }

        // Roll spawn count
        int spawnCount = 1;
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll < activeTable.OneEnemyChance) spawnCount = 1;
        else if (roll < activeTable.OneEnemyChance + activeTable.TwoEnemyChance) spawnCount = 2;
        else spawnCount = 3;

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnPoolConfig selectedPool = PickRandomPool(activeTable.Pools);
            if (selectedPool != null && selectedPool.Enemies != null && selectedPool.Enemies.Count > 0)
            {
                EnemySO rolledEnemy = PickRandomEnemy(selectedPool.Enemies);
                int rolledLevel = UnityEngine.Random.Range(selectedPool.MinLevel, selectedPool.MaxLevel + 1);
                if (rolledEnemy != null) 
                {
                    Enemy enemy = new Enemy(rolledEnemy, rolledLevel);
                    
                    // Roll for Unique status
                    if (UnityEngine.Random.Range(0f, 100f) <= selectedPool.UniqueChance)
                    {
                        // Get random prefix and suffix
                        EnemyPrefix prefix = (EnemyPrefix)UnityEngine.Random.Range(1, System.Enum.GetValues(typeof(EnemyPrefix)).Length);
                        EnemySuffix suffix = (EnemySuffix)UnityEngine.Random.Range(1, System.Enum.GetValues(typeof(EnemySuffix)).Length);
                        enemy.BecomeUnique(prefix, suffix);
                    }
                    
                    enemies.Add(enemy);
                }
            }
        }

        return enemies;
    }

    private SpawnPoolConfig PickRandomPool(List<SpawnPoolConfig> pools)
    {
        if (pools == null || pools.Count == 0) return null;

        float totalChance = 0;
        foreach (var p in pools) totalChance += p.SpawnChance;

        float r = UnityEngine.Random.Range(0f, totalChance);
        float currentChance = 0;
        foreach (var p in pools)
        {
            currentChance += p.SpawnChance;
            if (r <= currentChance) return p;
        }
        return pools[0];
    }

    private EnemySO PickRandomEnemy(List<EnemySpawnWeight> pool)
    {
        if (pool == null || pool.Count == 0) return Resources.Load<EnemySO>("Enemies/Skeleton");
        
        int totalWeight = 0;
        foreach (var e in pool) totalWeight += e.weight;
        
        int r = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var e in pool)
        {
            currentWeight += e.weight;
            if (r < currentWeight) return e.enemyData;
        }
        return pool[0].enemyData;
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
        }

        TextOutputter.Instance.OutputText("Encounter skipped via instant resolution.");
        CurrentBattle.Cleanup();
        CurrentBattle = null;
    }

    private Boss CreateBoss()
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int floor = dm.CurrentDungeonFloor;
        int room = dm.CurrentEncounterIndex;

        string bossName = "";
        if (floor == 40 && room == 10)
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

            bossName = availableBosses[UnityEngine.Random.Range(0, availableBosses.Count)];
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
                
                // Pale Moon Level 1: 10% Mana Recovery
                ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
                if (rm?.CurrentReligion is ChildrenOfThePaleMoon && rm.CurrentReligion.CurrentFaithLevel >= 1)
                {
                    int recovery = Mathf.RoundToInt(Pcm.GetMana().MaxValue * 0.10f);
                    Pcm.GetMana().Increase(recovery);
                    TextOutputter.Instance.OutputText($"Pale Moon's Graces: Recovered {recovery} Mana.");
                }

                Notify(EventType.EncounterResolve);
                RunManager.Instance.GetService<SaveManager>()?.SaveRun();
                break;
            case EventType.EnemyDefeated:
                if (subject is Boss boss)
                {
                    _defeatedBosses.Add(boss.BossData.BossName);
                }

                // Whispering Flame Lvl 4: Fervor (Kill enemy in 4 turns)
                if (CurrentBattle != null && CurrentBattle.TurnCount <= 4)
                {
                    ReligionManager religM = RunManager.Instance.GetService<ReligionManager>();
                    if (religM?.CurrentReligion is WhisperingFlame && religM.CurrentReligion.CurrentFaithLevel >= 4)
                    {
                        Pcm.GrantFervorStack();
                    }
                }

                RunManager.Instance.GetService<ReligionManager>()?.CurrentReligion?.OnEnemyDefeated(subject as Enemy);
                Notify(subject, EventType.EnemyDefeated);
                break;
            default:
                break;
        }
    }
}
