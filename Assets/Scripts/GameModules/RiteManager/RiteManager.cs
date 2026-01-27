using UnityEngine;
using System.Collections.Generic;

public class RiteManager : GameModule
{
    private const int k_maxCapacity = 11;
    private const int k_maxRitePoints = 40;
    public int CurrentCapacityMax{ get; private set; }
    public int RitePoints
    {
        get
        {
            return CalculateRitePointsFromScore();
        }
    }

    public List<Rite> ActiveRites { get; private set; } = new List<Rite>();

    public override void AttachDefaultObservers()
    {
        // none for now
    }

    public bool HasRite(string riteID)
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

    public void EquipRite(Rite newRite)
    {
        if (CanEquip(newRite.RiteID) == false)
        {
            Debug.LogWarning("Cannot add more rites. Maximum reached.");
            return;
        }
        ActiveRites.Add(newRite);
    }

    public void UnequipRite(Rite riteToRemove)
    {
        if (CanUnequip(riteToRemove))
        {
            ActiveRites.Remove(riteToRemove);
        }
        else
        {
            Debug.LogWarning("Rite not found in active rites.");
        }
    }

    public int CalculateRitePointsFromScore()
    {
        return (int) RunManager.Instance.GetService<ScoreManager>().CurrentScore / 500;
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
}
