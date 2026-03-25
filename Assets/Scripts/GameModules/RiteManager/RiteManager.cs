using UnityEngine;
using System.Collections.Generic;
using System;

public class RiteManager : GameModule
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

    public override void AttachDefaultObservers()
    {
        // none for now
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

    private HashSet<string> _unlockedRiteIDs = new HashSet<string>();

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
        _unlockedRiteIDs = new HashSet<string>(ids);
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
