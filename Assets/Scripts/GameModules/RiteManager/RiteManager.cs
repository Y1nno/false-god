using UnityEngine;
using System.Collections.Generic;
using System;

public class RiteManager : GameModule, IObserver
{
    private const int k_maxCapacity = 11;
    private const int k_maxRitePoints = 40;

    public int BaseRitePoints { get; set; } = 0; // Default 0 points
    public int CurrentCapacityMax{ get; private set; }
    public int RitePoints
    {
        get
        {
            return CalculateRitePointsFromScore();
        }
    }

    public RiteManager()
    {
        CurrentCapacityMax = k_maxCapacity;
    }

    public Dictionary<RiteType, Rite> ActiveRites { get; private set; } = new Dictionary<RiteType, Rite>();
    
    // Meta Tracking Data (cached from SaveManager)
    public int TotalDeaths;
    public int TotalSpellsCast;
    public int TotalMaxDiceRolls;
    public Dictionary<string, int> KillsByEnemyID = new Dictionary<string, int>();
    public HashSet<string> BossesDefeated = new HashSet<string>();
    public HashSet<string> ReligionsJoined = new HashSet<string>();
    public bool Spent1000Gold;
    public bool Used50Consumables;
    public string StartingRelicID = "";

    public override void AttachDefaultObservers()
    {
        RunManager.Instance.GetService<DungeonManager>()?.AttachObserver(this);
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.DungeonEncounterAdvance)
        {
            UpdateCooldowns();
        }
    }

    private void UpdateCooldowns()
    {
        foreach (var rite in ActiveRites.Values)
        {
            if (rite.CurrentCooldown > 0)
            {
                rite.CurrentCooldown--;
                if (rite.CurrentCooldown == 0)
                {
                    TextOutputter.Instance.OutputText($"Rite {rite.RiteID} is now ready!");
                }
            }
        }
    }

    public bool HasRite(string riteID)
    {
        return HasRite(Enum.Parse<RiteType>(riteID));
    }

    public bool HasRite(RiteType type)
    {
        return ActiveRites.ContainsKey(type);
    }

    public bool HasRite<T>() where T : Rite
    {
        foreach (var rite in ActiveRites.Values)
        {
            if (rite is T)
            {
                return true;
            }
        }
        return false;
    }

    public Rite GetRite(RiteType type)
    {
        if (ActiveRites.TryGetValue(type, out var rite))
        {
            return rite;
        }
        return null;
    }

    public T GetRite<T>() where T : Rite
    {
        foreach (var rite in ActiveRites.Values)
        {
            if (rite is T typedRite)
            {
                return typedRite;
            }
        }
        return null;
    }

    private List<string> _unlockedRiteIDs = new List<string>();


    public void UnlockRite(string riteID)
    {
        if (!_unlockedRiteIDs.Contains(riteID))
        {
            _unlockedRiteIDs.Add(riteID);
            TextOutputter.Instance.OutputText($"Unlocked Rite: {riteID}");
        }
    }

    public void LockRite(string riteID)
    {
        if (_unlockedRiteIDs.Contains(riteID))
        {
            _unlockedRiteIDs.Remove(riteID);
            TextOutputter.Instance.OutputText($"Locked Rite: {riteID}");
        }
    }

    public bool IsRiteUnlocked(string riteID)
    {
        return _unlockedRiteIDs.Contains(riteID);
    }

    public List<string> GetUnlockedRiteIDs() => new List<string>(_unlockedRiteIDs);
    
    public void SetUnlockedRiteIDs(List<string> ids)
    {
        _unlockedRiteIDs = new List<string>(ids);
    }

    public void RecordKill(string enemyID, bool isBoss)
    {
        if (string.IsNullOrEmpty(enemyID)) return;
        
        if (!KillsByEnemyID.ContainsKey(enemyID)) KillsByEnemyID[enemyID] = 0;
        KillsByEnemyID[enemyID]++;
        
        if (isBoss) BossesDefeated.Add(enemyID);
        
        CheckUnlocks();
    }

    public void RecordReligionJoin(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        ReligionsJoined.Add(id);
        CheckUnlocks();
    }

    public void RecordSpellCast()
    {
        TotalSpellsCast++;
        CheckUnlocks();
    }

    public void RecordMaxDiceRoll()
    {
        TotalMaxDiceRolls++;
        CheckUnlocks();
    }

    public void RecordDeath()
    {
        TotalDeaths++;
        CheckUnlocks();
        SaveMeta();
    }

    public void CheckUnlocks()
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        int floor = dm != null ? dm.HighestFloorReached : 1;

        // Candle: Depth 10
        if (floor >= 10) UnlockRite(RiteType.Candle.ToString());
        // Judgement: Depth 20
        if (floor >= 20) UnlockRite(RiteType.Judgement.ToString());
        // Juggernaut: Depth 30
        if (floor >= 30) UnlockRite(RiteType.Juggernaut.ToString());
        // Berserk: Depth 40
        if (floor >= 40) UnlockRite(RiteType.Berserk.ToString());
        
        // Colossus: 5 Stone Golems
        if (GetKillCount("Stone Golem") >= 5) UnlockRite(RiteType.Colossus.ToString());
        // Chalice: 10 Heretics
        if (GetKillCount("Heretic") >= 10) UnlockRite(RiteType.Chalice.ToString());
        // Beast: 200 Enemies total
        int totalKills = 0;
        foreach (var count in KillsByEnemyID.Values) totalKills += count;
        if (totalKills >= 200) UnlockRite(RiteType.Beast.ToString());

        // Lazarus: 5 Deaths
        if (TotalDeaths >= 5) UnlockRite(RiteType.Lazarus.ToString());
        // Merlin: 100 Spells
        if (TotalSpellsCast >= 100) UnlockRite(RiteType.Merlin.ToString());
        // Palamedes: 10 Max Rolls
        if (TotalMaxDiceRolls >= 10) UnlockRite(RiteType.Palamedes.ToString());

        // Economy/Consumables (Updated on Save/Run End)
        if (Spent1000Gold) UnlockRite(RiteType.Midas.ToString());
        if (Used50Consumables) UnlockRite(RiteType.Gluttony.ToString());

        // Ouroboros: Joined Serpent's Coil
        if (ReligionsJoined.Contains("Serpent's Coil")) UnlockRite(RiteType.Ouroboros.ToString());
        
        // Empress: Child of the Pale Moon (Special hook in Religion)
        
        // Afterbirth: Defeat Fallen Saint
        if (BossesDefeated.Contains("Fallen Saint")) UnlockRite(RiteType.Afterbirth.ToString());
        
        // Faithless: Depth 40 no religion (Special hook in SaveManager or DungeonManager)
    }

    private int GetKillCount(string id)
    {
        return KillsByEnemyID.ContainsKey(id) ? KillsByEnemyID[id] : 0;
    }

    private void SaveMeta()
    {
        SyncRunStats();
        RunManager.Instance.GetService<SaveManager>()?.SaveMeta();
    }

    public void SyncRunStats()
    {
        EconomyManager econ = RunManager.Instance.GetService<EconomyManager>();
        if (econ != null && econ.GoldSpentInRun >= 1000) Spent1000Gold = true;

        InventoryManager inv = RunManager.Instance.GetService<InventoryManager>();
        if (inv != null && inv.ConsumablesUsedInRun >= 50) Used50Consumables = true;
        
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (dm != null && dm.HighestFloorReached >= 40 && rm != null && rm.CurrentReligion == null && ReligionsJoined.Count == 0)
        {
            UnlockRite(RiteType.Faithless.ToString());
        }
        
        CheckUnlocks();
    }

    public void EquipRite(Rite newRite)
    {
        if (!IsRiteUnlocked(newRite.RiteID))
        {
            TextOutputter.Instance.OutputText($"Rite {newRite.RiteID} is locked!");
            return;
        }

        if (CalculateRitePointsRemaining() < newRite.RitePointCost)
        {
             TextOutputter.Instance.OutputText($"Not enough Rite Points! Cost: {newRite.RitePointCost}, Available: {CalculateRitePointsRemaining()}");
             return;
        }

        if (ActiveRites.Count >= CurrentCapacityMax)
        {
            Debug.LogWarning("Cannot add more rites. Maximum Capacity reached.");
            return;
        }

        if (HasRite(newRite.RiteID))
        {
             Debug.LogWarning($"Cannot add rite. {newRite.RiteID} is already equipped.");
             return;
        }
        ActiveRites.Add(newRite.RiteType, newRite);
        newRite.OnEquip(RunManager.Instance.GetService<PlayerManager>());
        TextOutputter.Instance.OutputText($"Equipped Rite: {newRite.RiteID}");
    }

    public void EquipRite(RiteType type)
    {
        Rite rite = RiteFactory.CreateRite(type);
        if (rite != null)
        {
            EquipRite(rite);
        }
        else
        {
            TextOutputter.Instance.OutputText($"Rite {type} is not implemented or factory failed.");
            Debug.LogWarning($"RiteFactory returned null for type: {type}");
        }
    }

    public void UnequipRite(Rite riteToRemove)
    {
        if (CanUnequip(riteToRemove))
        {
            ActiveRites.Remove(riteToRemove.RiteType);
            riteToRemove.OnUnequip(RunManager.Instance.GetService<PlayerManager>());
             TextOutputter.Instance.OutputText($"Unequipped Rite: {riteToRemove.RiteID}");
        }
        else
        {
            Debug.LogWarning("Rite not found in active rites.");
        }
    }

    public void UnequipRite(RiteType type)
    {
        // Find rite by type
        Rite toRemove = null;
        foreach (Rite r in ActiveRites.Values)
        {
            // Simple check: does the ID match the type name?
            // Converting Enum to String is okay for now as IDs match Enum names (Colossus, Beast, etc.)
            if (r.RiteID == type.ToString())
            {
                toRemove = r;
                break;
            }
        }

        if (toRemove != null)
        {
            UnequipRite(toRemove);
        }
        else
        {
            TextOutputter.Instance.OutputText($"Rite {type} is not equipped.");
        }
    }


    public int CalculateRitePointsFromScore()
    {
        return BaseRitePoints + (int) RunManager.Instance.GetService<ScoreManager>().TotalScore / 500;
    }

    public bool CanEquip(string riteID)
    {
        if (ActiveRites.Count >= CurrentCapacityMax)
        {
            return false;
        }
        if (HasRite(riteID))
        {
            return false;
        }
        return true;
    }

    public bool CanEquip(Rite rite)
    {
        if (CalculateRitePointsRemaining() < rite.RitePointCost)
        {
            return false;
        }
        return CanEquip(rite.RiteID);
    }

    public bool CanUnequip(string riteID)
    {
        Enum.TryParse<RiteType>(riteID, out var type);
        return CanUnequip(type);
    }

    public bool CanUnequip(Rite rite)
    {
        return CanUnequip(rite.RiteType);
    }

    public bool CanUnequip(RiteType type)
    {
        return ActiveRites.ContainsKey(type);
    }

    public int CalculateRitePointsRemaining()
    {
        int ritePoints = 0;
        foreach (Rite rite in ActiveRites.Values)
        {
            ritePoints += rite.RitePointCost;
        }
        return CalculateRitePointsFromScore() - ritePoints;
    }

    public float CalculateStatMultiplierFromRites(SecondaryStat stat)
    {
        float multiplier = 1.0f;
        switch (stat)
        {
            case SecondaryStat.PHATK:
                if (HasRite(RiteType.Berserk))
                {
                    multiplier += GetRite<BerserkRite>().PhysicalAttackMultiplier;
                }
                break;
            case SecondaryStat.SPDEF:
                if (HasRite(RiteType.Berserk))
                {
                    multiplier += GetRite<BerserkRite>().SpecialDefenseMultiplier;
                }
                break;
            case SecondaryStat.SPATK:
                if (HasRite(RiteType.Merlin))
                {
                    multiplier += GetRite<MerlinRite>().SpecialAttackMultiplier;
                }
                break;
            case SecondaryStat.PHDEF:
                if (HasRite(RiteType.Merlin))
                {
                    multiplier += GetRite<MerlinRite>().PhysicalDefenseMultiplier;
                }
                break;
        }
        return multiplier;
    }

    public float CalculateFlatStatBonus(SecondaryStat stat)
    {
        float flatBonus = 0.0f;
        switch (stat)
        {
            case SecondaryStat.CRIT:
                if (HasRite(RiteType.Judgement))
                {
                    flatBonus += GetRite<JudgementRite>().CritChance;
                }
                break;
        }
        return flatBonus;
    }

    public void AddBaseRitePoints(int amount)
    {
        BaseRitePoints += amount;
        TextOutputter.Instance.OutputText($"Added {amount} Rite Points. Total Base: {BaseRitePoints}");
    }

}
