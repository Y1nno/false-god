using UnityEngine;
using System.Collections.Generic;

public static class AilmentScaling
{
    private struct ScalingData
    {
        public int Duration;
        public float StatReduction; // e.g. 0.05 for 5%
        public float DamagePercent; // e.g. 0.02 for 2%
        public string SpecialEffect;
    }

    private static Dictionary<AilmentType, ScalingData[]> _tables = new Dictionary<AilmentType, ScalingData[]>
    {
        {
            AilmentType.Burn, new ScalingData[]
            {
                new ScalingData { Duration = 2, StatReduction = 0.05f, DamagePercent = 0.02f },
                new ScalingData { Duration = 2, StatReduction = 0.08f, DamagePercent = 0.03f },
                new ScalingData { Duration = 2, StatReduction = 0.10f, DamagePercent = 0.04f },
                new ScalingData { Duration = 3, StatReduction = 0.12f, DamagePercent = 0.05f },
                new ScalingData { Duration = 4, StatReduction = 0.14f, DamagePercent = 0.06f }
            }
        },
        {
            AilmentType.Poison, new ScalingData[]
            {
                new ScalingData { Duration = 2, StatReduction = 0.05f, DamagePercent = 0.05f },
                new ScalingData { Duration = 2, StatReduction = 0.08f, DamagePercent = 0.06f },
                new ScalingData { Duration = 2, StatReduction = 0.12f, DamagePercent = 0.07f },
                new ScalingData { Duration = 3, StatReduction = 0.15f, DamagePercent = 0.08f },
                new ScalingData { Duration = 4, StatReduction = 0.20f, DamagePercent = 0.10f }
            }
        },
        {
            AilmentType.Frozen, new ScalingData[]
            {
                new ScalingData { Duration = 1, StatReduction = 0.00f, DamagePercent = 0f, SpecialEffect = "None" },
                new ScalingData { Duration = 2, StatReduction = 0.00f, DamagePercent = 0f, SpecialEffect = "None" },
                new ScalingData { Duration = 2, StatReduction = -0.20f, DamagePercent = 0f, SpecialEffect = "+20% Def" }, // Negative reduction = boost
                new ScalingData { Duration = 2, StatReduction = -0.20f, DamagePercent = 0f, SpecialEffect = "+20% Def" }, // Placeholder for 4
                new ScalingData { Duration = 3, StatReduction = -0.30f, DamagePercent = 0f, SpecialEffect = "+30% Def" }
            }
        },
        {
            AilmentType.Bleed, new ScalingData[]
            {
                new ScalingData { Duration = 2, StatReduction = 0.20f, DamagePercent = 0.02f, SpecialEffect = "-20% Healing" },
                new ScalingData { Duration = 2, StatReduction = 0.25f, DamagePercent = 0.03f, SpecialEffect = "-25% Healing" },
                new ScalingData { Duration = 2, StatReduction = 0.30f, DamagePercent = 0.04f, SpecialEffect = "-30% Healing" },
                new ScalingData { Duration = 3, StatReduction = 0.35f, DamagePercent = 0.06f, SpecialEffect = "-35% Healing" },
                new ScalingData { Duration = 4, StatReduction = 0.50f, DamagePercent = 0.07f, SpecialEffect = "-50% Healing" }
            }
        },
        {
            AilmentType.Weaken, new ScalingData[]
            {
                new ScalingData { Duration = 2, StatReduction = 0.20f, DamagePercent = 0f },
                new ScalingData { Duration = 2, StatReduction = 0.25f, DamagePercent = 0f },
                new ScalingData { Duration = 2, StatReduction = 0.30f, DamagePercent = 0f },
                new ScalingData { Duration = 3, StatReduction = 0.35f, DamagePercent = 0f },
                new ScalingData { Duration = 3, StatReduction = 0.50f, DamagePercent = 0f }
            }
        },
        { AilmentType.Stun, new ScalingData[] { new ScalingData { Duration = 1 } } },
        { AilmentType.Charmed, new ScalingData[] { new ScalingData { Duration = 2 } } },
        { AilmentType.AtkBonus, new ScalingData[] { new ScalingData { Duration = 3, StatReduction = -0.40f } } },
        { AilmentType.Shielded, new ScalingData[] { new ScalingData { Duration = 3, StatReduction = -0.20f } } },
        { AilmentType.AtkDebuff, new ScalingData[] { new ScalingData { Duration = 2, StatReduction = 0.30f } } },
        { AilmentType.DefDebuff, new ScalingData[] { new ScalingData { Duration = 2, StatReduction = 0.30f } } }
    };

    private static ScalingData GetData(AilmentType type, int stacks)
    {
        if (!_tables.ContainsKey(type)) return new ScalingData { Duration = 1, StatReduction = 0, DamagePercent = 0 };
        int idx = Mathf.Clamp(stacks - 1, 0, _tables[type].Length - 1);
        return _tables[type][idx];
    }

    public static int GetDuration(AilmentType type, int stacks) => GetData(type, stacks).Duration;
    public static float GetStatModifier(AilmentType type, int stacks) => GetData(type, stacks).StatReduction;
    public static float GetDamagePercent(AilmentType type, int stacks) => GetData(type, stacks).DamagePercent;
}
