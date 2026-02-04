using UnityEngine;
using System.Collections.Generic;

public class Orc : Enemy
{
    private readonly static string k_name = "Orc";
    private readonly static int k_baseHealth = 20;
    private readonly static int k_baseMana = 0;
    private readonly static Dictionary<Stat, int> k_baseStats = new Dictionary<Stat, int>()
    {
        { Stat.STR, 15 },
        { Stat.DEX, 10 },
        { Stat.INT, 5 },
        { Stat.SPD, 8 }
    };
     private readonly static List<int> k_availableActionIDs = new List<int>()
    {
        01, // Basic Attack
    };

    public Orc() :  base(name: k_name, initialStats: k_baseStats, initialHealth: k_baseHealth, initialMana: k_baseMana, availableActionIDs: k_availableActionIDs)
    {

    }
}