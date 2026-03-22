using UnityEngine;
using System.Collections.Generic;
using System;

public class EncounterManager : GameModule, IObserver
{
    private Encounter _currentEncounter;
    private Encounter _forcedNextEncounter;

    private float _treasureChance = 5.0f;
    private float _trapChance = 10.0f;
    private float _fountainChance = 5.0f;
    private float _shrineChance = 5.0f;

    private int _totalEncountersResolved = 0;
    private bool _religionPending = false;
    private bool _shopPending = false;

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
        
        if (_forcedNextEncounter != null)
        {
            _currentEncounter = _forcedNextEncounter;
            _forcedNextEncounter = null;
        }
        else
        {
            TrapEncounterSO selectedTrap = null;
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
        int currentTotalIndex = _totalEncountersResolved + 1;
        if (currentTotalIndex % 20 == 0) _religionPending = true;
        if (currentTotalIndex % 25 == 0) _shopPending = true;

        // 3. Religion Milestone (Fixed trigger, delayed if Boss/Rest)
        if (_religionPending)
        {
            return EncounterType.Religious;
        }

        // 4. Shop Milestone (Merchant/Blacksmith 50/50, delayed if Boss/Rest)
        if (_shopPending)
        {
            // Note: Merchant/Blacksmith/Fountain logic to be implemented later as per user request
            return UnityEngine.Random.value <= 0.5f ? EncounterType.Merchant : EncounterType.Blacksmith;
        }

        // 5. Escalated Chance Rolls
        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0;

        // Chest (5% base, +2.5% increment)
        cumulative += _treasureChance;
        if (roll <= cumulative) return EncounterType.Treasure;

        // Trap (10% base, +2% increment)
        cumulative += _trapChance;
        if (roll <= cumulative) return EncounterType.Trap;

        // Fountain (5% base, +1.5% increment)
        cumulative += _fountainChance;
        if (roll <= cumulative) return EncounterType.Fountain;

        // Health Shrine (5% base, +1.5% increment)
        cumulative += _shrineChance;
        if (roll <= cumulative) return EncounterType.Religious; // Using Religious as surrogate for Shrine for now

        // 6. Fallback (Enemy)
        return EncounterType.Enemy;
    }

    private void HandleEncounterChances(EncounterType encounterType)
    {
        // Reset current chances if they just spawned
        if (encounterType == EncounterType.Treasure) _treasureChance = 5.0f;
        else _treasureChance += 2.5f;

        if (encounterType == EncounterType.Trap) _trapChance = 10.0f;
        else _trapChance += 2.0f;

        if (encounterType == EncounterType.Fountain) _fountainChance = 5.0f;
        else _fountainChance += 1.5f;

        if (encounterType == EncounterType.Religious) _shrineChance = 5.0f; // Shrine reset
        else _shrineChance += 1.5f;

        // Reset milestone pendings if they spawned
        if (encounterType == EncounterType.Religious) _religionPending = false;
        if (encounterType == EncounterType.Merchant || encounterType == EncounterType.Blacksmith) _shopPending = false;
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
            _totalEncountersResolved++;
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
