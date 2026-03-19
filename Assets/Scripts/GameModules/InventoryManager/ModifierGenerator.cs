using UnityEngine;
using System.Collections.Generic;

public static class ModifierGenerator
{
    private static ModifierPoolSO _pool;

    private static void LoadPool()
    {
        if (_pool == null)
        {
            _pool = Resources.Load<ModifierPoolSO>("ModifierPool");
            if (_pool == null)
            {
                Debug.LogWarning("ModifierPoolSO not found in Resources. Modifiers will not be applied.");
            }
        }
    }

    public static void ApplyRandomModifiers(Equipment equipment)
    {
        LoadPool();
        if (_pool == null) return;

        int prefixCount = 0;
        int suffixCount = 0;

        // Rarity-based slot counts
        switch (equipment.Rarity)
        {
            case Rarity.Common:
                prefixCount = 0; suffixCount = 0;
                break;
            case Rarity.Uncommon:
                prefixCount = Random.Range(0, 2); // 0-1
                suffixCount = Random.Range(0, 2); // 0-1
                break;
            case Rarity.Rare:
                prefixCount = Random.Range(1, 3); // 1-2
                suffixCount = Random.Range(1, 3); // 1-2
                break;
            case Rarity.Epic:
                prefixCount = 2; suffixCount = 1; // 2 prefixes, 1 suffix
                break;
            case Rarity.Legendary:
                prefixCount = 2; suffixCount = 2;
                break;
        }

        List<ModifierData> prefixPool = null;
        List<ModifierData> suffixPool = null;

        if (equipment.IsCatalyst)
        {
            prefixPool = _pool.CatalystPrefixes;
            suffixPool = _pool.CatalystSuffixes;
        }
        else if (equipment.Slot == EquipmentSlot.Weapon)
        {
            prefixPool = _pool.WeaponPrefixes;
            suffixPool = _pool.WeaponSuffixes;
        }
        else
        {
            prefixPool = _pool.ArmorPrefixes;
            suffixPool = _pool.ArmorSuffixes;
        }

        RollAndAdd(equipment, equipment.Prefixes, prefixPool, prefixCount);
        RollAndAdd(equipment, equipment.Suffixes, suffixPool, suffixCount);
    }

    private static void RollAndAdd(Equipment equipment, List<ModifierInstance> destination, List<ModifierData> pool, int count)
    {
        if (pool == null || pool.Count == 0 || count <= 0) return;
        
        List<ModifierData> available = new List<ModifierData>(pool);
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int index = Random.Range(0, available.Count);
            ModifierInstance mi = available[index].Roll();
            destination.Add(mi);
            
            // If the modifier introduces a trait, append it to the item's runtime trait list
            if (mi.EffectType == ModifierEffectType.Trait)
            {
                equipment.Traits.Add(new TraitWithValue(mi.Trait, mi.Value)
                {
                    CooldownType = mi.CooldownType,
                    CooldownDuration = mi.CooldownDuration
                });
            }
            
            available.RemoveAt(index); // Prevent duplicate modifiers of the same name on the same item
        }
    }
}
