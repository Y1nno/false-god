using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ModifierPool", menuName = "Scriptable Objects/Items/Modifier Pool")]
public class ModifierPoolSO : ScriptableObject
{
    [Header("Weapon Modifiers")]
    public List<ModifierData> WeaponPrefixes;
    public List<ModifierData> WeaponSuffixes;

    [Header("Armor Modifiers")]
    public List<ModifierData> ArmorPrefixes;
    public List<ModifierData> ArmorSuffixes;

    [Header("Catalyst Modifiers")]
    public List<ModifierData> CatalystPrefixes;
    public List<ModifierData> CatalystSuffixes;
}

[System.Serializable]
public struct ModifierData
{
    public string Name;
    public ModifierEffectType EffectType;
    public EquipmentStat TargetStat;
    public int MinValue;
    public int MaxValue;
    public EquipmentTrait Trait;
    
    public CooldownType CooldownType;
    public int CooldownDuration;

    public ModifierInstance Roll()
    {
        int rolledValue = Random.Range(MinValue, MaxValue + 1);
        if (EffectType == ModifierEffectType.Trait)
        {
            var mi = new ModifierInstance(Name, Trait, rolledValue);
            mi.CooldownType = CooldownType;
            mi.CooldownDuration = CooldownDuration;
            return mi;
        }
        return new ModifierInstance(Name, EffectType, rolledValue, TargetStat);
    }
}
