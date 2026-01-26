using UnityEngine;
using System.Collections.Generic;

public class ManagerBox
{
    private CommandManager _commandManager;
    private CombatManager _combatManager;

    private DungeonManager _dungeonManager;
    private EncounterManager _encounterManager;
    private EconomyManager _economyManager;

    private InventoryManager _inventoryManager;
    private PlayerManager _playerManager;
    private ReligionManager _religionManager;
    private RiteManager _riteManager;
    private SaveManager _saveManager;
    private XPManager _xpManager;

    private List<GameModule> _allManagers = new List<GameModule>();

    public void InitializeAllManagers()
    {
        _commandManager = new CommandManager();
        _combatManager = new CombatManager();

        _dungeonManager = new DungeonManager();
        _encounterManager = new EncounterManager();
        _economyManager = new EconomyManager();

        _inventoryManager = new InventoryManager();
        _playerManager = new PlayerManager();
        _religionManager = new ReligionManager();
        _riteManager = new RiteManager();
        _saveManager = new SaveManager();
        _xpManager = new XPManager();

        _allManagers.Add(_commandManager);
        _allManagers.Add(_combatManager);
        _allManagers.Add(_dungeonManager);
        _allManagers.Add(_encounterManager);
        _allManagers.Add(_economyManager);
        _allManagers.Add(_inventoryManager);
        _allManagers.Add(_playerManager);
        _allManagers.Add(_religionManager);
        _allManagers.Add(_riteManager);
        _allManagers.Add(_saveManager);
        _allManagers.Add(_xpManager);

        SetupObservers();
    }

    private void SetupObservers()
    {
        foreach (var manager in _allManagers)
        {
            manager.AttachDefaultObservers();
        }
    }

    public T GetManager<T>() where T : class
    {
        if (typeof(T) == typeof(CommandManager))
            return _commandManager as T;
        if (typeof(T) == typeof(CombatManager))
            return _combatManager as T;
        if (typeof(T) == typeof(DungeonManager))
            return _dungeonManager as T;
        if (typeof(T) == typeof(EncounterManager))
            return _encounterManager as T;
        if (typeof(T) == typeof(EconomyManager))
            return _economyManager as T;
        if (typeof(T) == typeof(InventoryManager))
            return _inventoryManager as T;
        if (typeof(T) == typeof(PlayerManager))
            return _playerManager as T;
        if (typeof(T) == typeof(ReligionManager))
            return _religionManager as T;
        if (typeof(T) == typeof(RiteManager))
            return _riteManager as T;
        if (typeof(T) == typeof(SaveManager))
            return _saveManager as T;
        if (typeof(T) == typeof(XPManager))
            return _xpManager as T;

        return null;
    }
}
