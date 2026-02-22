using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyStatBox : StatBox
{
    private Dictionary<SecondaryStat, int> _secondaryStats = new Dictionary<SecondaryStat, int>();
    public EnemyStatBox(Dictionary<Stat, int> initialStats = null, Dictionary<SecondaryStat, int> initialSecondaryStats = null) : base(initialStats)
    {
        if (initialSecondaryStats != null)
        {
            _secondaryStats = new Dictionary<SecondaryStat, int>(initialSecondaryStats);
        }
    }

    public int GetSecondaryStat(SecondaryStat stat)
    {
        if (_secondaryStats.TryGetValue(stat, out int value))
        {
            return value;
        }
        return 0; // Default value if the stat is not found
    }
}
