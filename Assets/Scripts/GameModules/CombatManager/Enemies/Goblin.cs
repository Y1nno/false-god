using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{
    private readonly static string k_name = "Goblin";
    private readonly static int k_baseHealth = 10;
    private readonly static int k_baseMana = 20;
    private readonly static Dictionary<Stat, int> k_baseStats = new Dictionary<Stat, int>()
    {
        { Stat.STR, 10 },
        { Stat.DEX, 15 },
        { Stat.INT, 5 },
        { Stat.SPD, 12 }
    };
    private readonly static List<int> k_availableActionIDs = new List<int>()
    {
        01, // Basic Attack
    };

    private readonly static int k_goldValue = 5;

    public Goblin() : base(name: k_name, initialStats: k_baseStats, initialHealth: k_baseHealth, initialMana: k_baseMana, availableActionIDs: k_availableActionIDs, goldValue: k_goldValue)
    {}
}
