using UnityEngine;
using System.Collections.Generic;
using System;

public class EncounterManager : GameModule, IObserver
{
    private Encounter _currentEncounter;
    private Encounter _forcedNextEncounter;

    public float TreasureChance = 5.0f;
    public float TrapChance = 10.0f;
    public float FountainChance = 5.0f;
    public float ShrineChance = 5.0f;

    public int TotalEncountersResolved = 0;
    public bool ReligionPending = false;
    public bool ShopPending = false;
    public int EncountersSinceLastEscalation = 0;

    private EncounterType _currentEncounterType;
    private string _currentTrapSOID;

    private List<TrapEncounterSO> _availableTrapEncounters;

    private DungeonManager _dm = RunManager.Instance.GetService<DungeonManager>();

    #region Public API

    public override void AttachDefaultObservers()
    {
        _availableTrapEncounters = new List<TrapEncounterSO>();
        
#if UNITY_EDITOR
        string[] trapGuids = UnityEditor.AssetDatabase.FindAssets("t:TrapEncounterSO");
        foreach(string guid in trapGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TrapEncounterSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<TrapEncounterSO>(path);
            if (so != null)
            {
                _availableTrapEncounters.Add(so);
            }
        }
#else
        _availableTrapEncounters.AddRange(Resources.LoadAll<TrapEncounterSO>("Encounters/Traps"));
#endif

        if (_availableTrapEncounters == null || _availableTrapEncounters.Count == 0)
        {
            Debug.LogWarning("EncounterManager: No TrapEncounterSOs found in project (AssetDatabase or Resources)!");
        }
    }

    public void CreateEncounter()
    {
        EncounterType encounterType = DetermineEncounterType();
        HandleEncounterChances(encounterType);
        
        TrapEncounterSO selectedTrap = null;
        if (_forcedNextEncounter != null)
        {
            _currentEncounter = _forcedNextEncounter;
            _forcedNextEncounter = null;
        }
        else
        {
            if (encounterType == EncounterType.Trap && _availableTrapEncounters != null && _availableTrapEncounters.Count > 0)
            {
                float totalSpawnChance = 0;
                foreach (var trap in _availableTrapEncounters)
                {
                    totalSpawnChance += trap.SpawnChance;
                }

                float randomValue = UnityEngine.Random.Range(0, totalSpawnChance);
                float cumulativeChance = 0;

                foreach (var trap in _availableTrapEncounters)
                {
                    cumulativeChance += trap.SpawnChance;
                    if (randomValue <= cumulativeChance)
                    {
                        selectedTrap = trap;
                        break;
                    }
                }
            }
            _currentEncounter = new EncounterFactory().CreateEncounter(_dm.GetCurrentDungeonFloorData(), encounterType, selectedTrap);
        }
        
        _currentEncounterType = encounterType;
        _currentTrapSOID = selectedTrap != null ? selectedTrap.name : null;

        AttachToEncounter(_currentEncounter);
        TextOutputter.Instance.OutputText("Encounter created: " + _currentEncounter.GetType().Name);
        _currentEncounter.StartEncounter();
    }

    public void ForceNextEncounter(Encounter encounter)
    {
        _forcedNextEncounter = encounter;
    }

    public void ReplaceCurrentEncounter(EncounterType type)
    {
        if (_currentEncounter != null)
        {
            _currentEncounter.DetachObserver(this);
            // We do NOT resolve the old encounter, just discard it. The new one will take its place.
        }

        TrapEncounterSO selectedTrap = null;
        if (type == EncounterType.Trap && _availableTrapEncounters != null && _availableTrapEncounters.Count > 0)
        {
            float totalSpawnChance = 0;
            foreach (var trap in _availableTrapEncounters)
            {
                totalSpawnChance += trap.SpawnChance;
            }

            float randomValue = UnityEngine.Random.Range(0, totalSpawnChance);
            float cumulativeChance = 0;

            foreach (var trap in _availableTrapEncounters)
            {
                cumulativeChance += trap.SpawnChance;
                if (randomValue <= cumulativeChance)
                {
                    selectedTrap = trap;
                    break;
                }
            }
        }

        _currentEncounter = new EncounterFactory().CreateEncounter(_dm.GetCurrentDungeonFloorData(), type, selectedTrap);
        _currentEncounterType = type;
        _currentTrapSOID = selectedTrap != null ? selectedTrap.name : null;

        AttachToEncounter(_currentEncounter);
        TextOutputter.Instance.OutputText("Encounter switched to: " + _currentEncounter.GetType().Name);
        _currentEncounter.StartEncounter();
    }

    public void BeginEncounter()
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        TextOutputter.Instance.OutputText("Encounter started: " + _currentEncounter.GetType().Name);
        _currentEncounter.StartEncounter();
    }

    public void ResolveEncounter()
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        TextOutputter.Instance.OutputText("Encounter resolved: " + _currentEncounter.GetType().Name);
        _currentEncounter.ResolveEncounter();
    }

    public Encounter GetCurrentEncounter()
    {
        return _currentEncounter;
    }

    public EncounterType GetCurrentEncounterType() => _currentEncounterType;
    public string GetCurrentTrapSOID() => _currentTrapSOID;

    public void RestoreState(RunSaveData data)
    {
        _currentEncounterType = data.CurrentEncounterType;
        _currentTrapSOID = data.CurrentTrapSOID;
        TotalEncountersResolved = data.TotalEncountersResolved;
        ReligionPending = data.ReligionPending;
        ShopPending = data.ShopPending;
        EncountersSinceLastEscalation = data.EncountersSinceLastEscalation;
        TreasureChance = data.TreasureChance;
        TrapChance = data.TrapChance;
        FountainChance = data.FountainChance;
        ShrineChance = data.ShrineChance;

        TrapEncounterSO selectedTrap = null;
        if (data.CurrentEncounterType == EncounterType.Trap && !string.IsNullOrEmpty(data.CurrentTrapSOID))
        {
            selectedTrap = _availableTrapEncounters.Find(t => t.name == data.CurrentTrapSOID);
        }

        _currentEncounter = new EncounterFactory().CreateEncounter(_dm.GetCurrentDungeonFloorData(), data.CurrentEncounterType, selectedTrap);
        
        if (_currentEncounter is EnemyEncounter enemyEnc && data.CurrentEnemyIDs != null && data.CurrentEnemyIDs.Count > 0)
        {
            RunManager.Instance.GetService<CombatManager>()?.PrepareRestoredBattle(data);
        }
        else if (_currentEncounter is MerchantEncounter me)
        {
            me.RestoreState(data.ShopInventory);
        }
        else if (_currentEncounter is BlacksmithEncounter be)
        {
            be.RestoreState(data.ShopInventory);
        }

        AttachToEncounter(_currentEncounter);
        _currentEncounter.StartEncounter();
    }
    #endregion

    private EncounterType DetermineEncounterType()
    {
        DungeonFloorData floorData = _dm.GetCurrentDungeonFloorData();
        int floor = floorData.Floor;
        int encounterIndex = floorData.Room;
        int totalEncountersInFloor = floorData.TotalEncountersInRoom;

        // 1. Boss: Last encounter of depth 10, 20, 30...
        if (encounterIndex == totalEncountersInFloor && floor % 10 == 0)
        {
            return EncounterType.Boss;
        }

        // 2. Rest: First encounter of floor starting from depth 2+
        if (encounterIndex == 1 && floor >= 2)
        {
            return EncounterType.Rest;
        }

        // --- Milestone Checks (Religion every 20, Shop every 25) ---
        // Since DetermineEncounterType is called BEFORE resolution, we check (_totalEncountersResolved + 1)
        int currentTotalIndex = TotalEncountersResolved + 1;
        if (currentTotalIndex % 20 == 0) ReligionPending = true;
        if (currentTotalIndex % 25 == 0) ShopPending = true;

        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (rm != null && rm.HasQuestItemForCurrentReligion()) ReligionPending = true;

        // 3. Religion Milestone (Fixed trigger, delayed if Boss/Rest)
        if (ReligionPending)
        {
            return EncounterType.Religious;
        }

        // 4. Shop Milestone (Merchant/Blacksmith 50/50, delayed if Boss/Rest)
        if (ShopPending)
        {
            // Note: Merchant/Blacksmith/Fountain logic to be implemented later as per user request
            return UnityEngine.Random.value <= 0.5f ? EncounterType.Merchant : EncounterType.Blacksmith;
        }

        // 5. Escalated Chance Rolls
        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0;

        // Chest (5% base, +2.5% increment)
        cumulative += TreasureChance;
        if (roll <= cumulative) return EncounterType.Treasure;

        // Trap (10% base, +2% increment)
        cumulative += TrapChance;
        if (roll <= cumulative) return EncounterType.Trap;

        // Fountain (5% base, +1.5% increment)
        cumulative += FountainChance;
        if (roll <= cumulative) return EncounterType.Fountain;

        // Health Shrine (5% base, +1.5% increment)
        cumulative += ShrineChance;
        if (roll <= cumulative) return EncounterType.Shrine;

        // 6. Fallback (Enemy)
        return EncounterType.Enemy;
    }

    private void HandleEncounterChances(EncounterType encounterType)
    {
        EncountersSinceLastEscalation++;
        bool shouldEscalate = EncountersSinceLastEscalation % 2 == 0;

        // Reset current chances if they just spawned, or escalate every other encounter
        if (encounterType == EncounterType.Treasure) TreasureChance = 5.0f;
        else if (shouldEscalate) TreasureChance += 2.5f;

        if (encounterType == EncounterType.Trap) TrapChance = 10.0f;
        else if (shouldEscalate) TrapChance += 2.0f;

        if (encounterType == EncounterType.Fountain) FountainChance = 5.0f;
        else if (shouldEscalate) FountainChance += 1.5f;

        if (encounterType == EncounterType.Shrine) ShrineChance = 5.0f;
        else if (shouldEscalate) ShrineChance += 1.5f;

        // Reset milestone pendings if they spawned
        if (encounterType == EncounterType.Religious) ReligionPending = false;
        if (encounterType == EncounterType.Merchant || encounterType == EncounterType.Blacksmith) ShopPending = false;
    }

    public void ProcessEncounterDecision(int decisionIndex)
    {
        if (_currentEncounter == null)
        {
            NullReferenceException  ex = new NullReferenceException("No encounter has been created.");
            Debug.LogException(ex);
            return;
        }
        _currentEncounter.RecieveDecision(decisionIndex);
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.DungeonRoomAdvance || eventType == EventType.DungeonEncounterAdvance)
        {
            //Debug.Log($"EncounterManager received {eventType}. Creating encounter...");
            CreateEncounter();
        }
        else if (eventType == EventType.EncounterResolve)
        {
            TotalEncountersResolved++;
            RunManager.Instance.GetService<EquipmentManager>()?.TickCooldowns(CooldownType.Encounters);

            // Propagate the event to EncounterManager's observers (like Rites)
            Notify(EventType.EncounterResolve);
            
            // Clean up observer from the SPECIFIC encounter that just resolved
            // (Note: _currentEncounter might already be the NEXT encounter if DungeonManager advanced first)
            if (subject is Encounter resolvedEncounter)
            {
                resolvedEncounter.DetachObserver(this);
            }
        }
        else if (eventType == EventType.EncounterStart)
        {
            Notify(EventType.EncounterStart);
        }
    }

    private void AttachToEncounter(Encounter encounter)
    {
        encounter.AttachObserver(this);
    }
}

public enum EncounterType
{
    Enemy,
    Treasure,
    Trap,
    NPC,
    Rest,
    Religious,
    Blacksmith,
    Merchant,
    Portal,
    Boss,
    Fountain,
    Shrine
}
