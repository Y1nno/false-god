using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ModifierPoolGenerator
{
    [MenuItem("Tools/Generate Modifier Pool")]
    public static void Generate()
    {
        ModifierPoolSO pool = ScriptableObject.CreateInstance<ModifierPoolSO>();

        // Weapon Prefixes
        pool.WeaponPrefixes = new List<ModifierData> {
            new ModifierData { Name = "Brutal", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.PhysicalAttack, MinValue = 5, MaxValue = 10 },
            new ModifierData { Name = "Savage", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.PhysicalAttack, MinValue = 10, MaxValue = 15 },
            new ModifierData { Name = "Keen", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.CritChance, MinValue = 5, MaxValue = 10 },
            new ModifierData { Name = "Jagged", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.SPD, MinValue = 8, MaxValue = 20 },
            new ModifierData { Name = "Vampiric", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.SoulSteal }
        };

        // Weapon Suffixes
        pool.WeaponSuffixes = new List<ModifierData> {
            new ModifierData { Name = "Venom", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.PoisonHit, MinValue = 5, MaxValue = 15 },
            new ModifierData { Name = "Embers", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.BurnHit, MinValue = 5, MaxValue = 15 },
            new ModifierData { Name = "Frost", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.FreezeHit, MinValue = 5, MaxValue = 15 },
            new ModifierData { Name = "Decay", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.WeakenHit, MinValue = 5, MaxValue = 15 }
        };

        // Armor Prefixes
        pool.ArmorPrefixes = new List<ModifierData> {
            new ModifierData { Name = "Reinforced", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.PhysicalDefense, MinValue = 3, MaxValue = 6 },
            new ModifierData { Name = "Fortified", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.MaxHP, MinValue = 10, MaxValue = 15 },
            new ModifierData { Name = "Agile", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.SPD, MinValue = 2, MaxValue = 5 },
            new ModifierData { Name = "Mystic", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.SpecialDefense, MinValue = 4, MaxValue = 8 },
            new ModifierData { Name = "Runic", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.MaxMana, MinValue = 10, MaxValue = 20 }
        };

        // Armor Suffixes
        pool.ArmorSuffixes = new List<ModifierData> {
            new ModifierData { Name = "Vitality", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.HealthRegen, MinValue = 2, MaxValue = 5 }, 
            new ModifierData { Name = "Protection", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.PhysicalDefense, MinValue = 3, MaxValue = 5 },
            new ModifierData { Name = "Thorns", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.DamageReflect, MinValue = 12, MaxValue = 12 },
            new ModifierData { Name = "Shadows", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.DodgeChance, MinValue = 6, MaxValue = 15 },
            new ModifierData { Name = "Cleansing", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.AilmentResist, MinValue = 10, MaxValue = 20 },
            new ModifierData { Name = "Resilience", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.AilmentDurationReduction, MinValue = 50, MaxValue = 50 }
        };

        // Catalyst Prefixes 
        pool.CatalystPrefixes = new List<ModifierData> {
            new ModifierData { Name = "Arcane", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.SpecialAttack, MinValue = 4, MaxValue = 8 },
            new ModifierData { Name = "Charged", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.AttackMultiplier },
            new ModifierData { Name = "Mystic", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.INT, MinValue = 4, MaxValue = 10 },
            new ModifierData { Name = "Runed", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.DoubleStrike },
            new ModifierData { Name = "Refined", EffectType = ModifierEffectType.PercentStat, TargetStat = EquipmentStat.SpecialAttack, MinValue = 20, MaxValue = 45 }, // Placeholder for Mana cost
            new ModifierData { Name = "Cursed", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.SoulSteal } // Placeholder for random spell
        };

        // Catalyst Suffixes
        pool.CatalystSuffixes = new List<ModifierData> {
            new ModifierData { Name = "Insight", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.SoulSteal }, // Placeholder for mana on kill
            new ModifierData { Name = "Weakening", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.WeakenHit, MinValue = 10, MaxValue = 30 },
            new ModifierData { Name = "Vampirism", EffectType = ModifierEffectType.Trait, Trait = EquipmentTrait.SoulSteal },
            new ModifierData { Name = "Focus", EffectType = ModifierEffectType.FlatStat, TargetStat = EquipmentStat.CritChance, MinValue = 10, MaxValue = 25 }
        };

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(pool, "Assets/Resources/ModifierPool.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Modifier Pool Generated!");
    }
}
