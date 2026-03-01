using UnityEngine;
using System.Collections.Generic;
using System;

public class EncounterManager : GameModule, IObserver
{
    private Encounter _currentEncounter;
    private Encounter _forcedNextEncounter;

    private float _treasureEncounterChance = 0.0f;
    private float k_treasureEncounterIncrement = 0.015f;
    private float _trapEncounterChance = 0.0f;
    private float k_trapEncounterIncrement = 0.025f;

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
                selectedTrap = _availableTrapEncounters[UnityEngine.Random.Range(0, _availableTrapEncounters.Count)];
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
            selectedTrap = _availableTrapEncounters[UnityEngine.Random.Range(0, _availableTrapEncounters.Count)];
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
        int room = floorData.Room;

        //Every 20 encounters is a religious encounter
        if (floor % 2 == 0 && room == 10)
        {
            return EncounterType.Religious;
        }
        // Every 10 encounters is an NPC encounter
        else if (room == 10)
        {
            return EncounterType.NPC;
        }
        // The last room on the floor is always a rest encounter
        else if (room % 11 ==0)
        {
            return EncounterType.Rest;
        }
        else if (UnityEngine.Random.value <= _trapEncounterChance)
        {
            return EncounterType.Trap;
        }
        else if (UnityEngine.Random.value <= _treasureEncounterChance)
        {
            return EncounterType.Treasure;
        }
        else
        {
            return EncounterType.Enemy;
        }
    }

    private void HandleEncounterChances(EncounterType encounterType)
    {
        switch (encounterType)
        {
            case EncounterType.Treasure:
                _treasureEncounterChance = 0.0f;
                _trapEncounterChance += k_trapEncounterIncrement;
                break;
            case EncounterType.Trap:
                _trapEncounterChance = 0.0f;
                _treasureEncounterChance += k_treasureEncounterIncrement;
                break;
            default:
                _treasureEncounterChance += k_treasureEncounterIncrement;
                _trapEncounterChance += k_trapEncounterIncrement;
                break;
        }
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
        if (eventType == EventType.DungeonRoomAdvance)
        {
            //Debug.Log("EncounterManager received DungeonRoomAdvance notification.");
            CreateEncounter();
        }
        else if (eventType == EventType.EncounterResolve)
        {
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
    Religious
}
