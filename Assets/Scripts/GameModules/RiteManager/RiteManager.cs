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

    public List<Rite> ActiveRites { get; private set; } = new List<Rite>();

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
        foreach (Rite rite in ActiveRites)
        {
            if (rite.RiteType == type)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasRite<T>() where T : Rite
    {
        foreach (Rite rite in ActiveRites)
        {
            if (rite is T)
            {
                return true;
            }
        }
        return false;
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
        ActiveRites.Add(newRite);
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
            ActiveRites.Remove(riteToRemove);
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
        foreach (Rite r in ActiveRites)
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
        return BaseRitePoints + (int) RunManager.Instance.GetService<ScoreManager>().CurrentScore / 500;
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
        foreach (Rite rite in ActiveRites)
        {
            if (rite.RiteID == riteID)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanUnequip(Rite rite)
    {
        return ActiveRites.Contains(rite);
    }

    public int CalculateRitePointsRemaining()
    {
        int ritePoints = 0;
        foreach (Rite rite in ActiveRites)
        {
            ritePoints += rite.RitePointCost;
        }
        return CalculateRitePointsFromScore() - ritePoints;
    }

    public float CalculateStatMultiplierFromRites(SecondaryStat stat)
    {
        float multiplier = 1.0f;
        //TODO: Implement this when merged with branch that HasRite and GetRite are implemented
        /**
        switch (stat)
        {
            case SecondaryStat.PHATK:
                if (HasRite(RiteType.Berserk))
                {
                    multiplier += GetRite(RiteType.Berserk).PhysicalAttackMultiplier;
                }
                break;
            case SecondaryStat.SPDEF:
                if (HasRite(RiteType.Berserk))
                {
                    multiplier += GetRite(RiteType.Berserk).SpecialDefenseMultiplier;
                }
                break;
            case SecondaryStat.SPATK:
                if (HasRite(RiteType.Merlin))
                {
                    multiplier += GetRite(RiteType.Merlin).SpecialAttackMultiplier;
                }
                break;
            case SecondaryStat.PHDEF:
                if (HasRite(RiteType.Merlin))
                {
                    multiplier += GetRite(RiteType.Merlin).SpecialAttackMultiplier;
                }
                break;
        }
        **/
        return multiplier;
    }

    public float CalculateFlatStatBonus(SecondaryStat stat)
    {
        float flatBonus = 0.0f;
        //TODO: Implement this when merged with branch that HasRite and GetRite are implemented
        /**
        switch (stat)
        {
            case SecondaryStat.CRIT:
                if (HasRite(RiteType.Judgement))
                {
                    flatBonus += GetRite(RiteType.Judgement).CritChance;
                }
                break;
        }
        **/
        return flatBonus;
    }

    public void AddBaseRitePoints(int amount)
    {
        BaseRitePoints += amount;
        TextOutputter.Instance.OutputText($"Added {amount} Rite Points. Total Base: {BaseRitePoints}");
    }

    public Rite GetRite(RiteType type)
    {
        foreach (Rite rite in ActiveRites)
        {
            if (rite.RiteType == type)
            {
                return rite;
            }
        }
        return null;
    }
}
