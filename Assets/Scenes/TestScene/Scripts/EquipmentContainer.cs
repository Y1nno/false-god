using UnityEngine;
using TMPro;

public class EquipmentContainer : MonoBehaviour, IObserver
{
    private string _content;
    private RunManager _rm = null;
    private EquipmentManager _eqm = null;

    public TMP_Text textBox;

    public void Refresh()
    {
        if (_rm == null)
        {
            HookUpManagers();
        }
        _content = "Equipment:\n\n";

        AddEquipmentInfo();

        textBox.text = _content;
    }

    private void AddEquipmentInfo()
    {
        if (_eqm == null)
        {
             _content += "No Equipment Manager connected.\n";
             return;
        }

        // We want to list all possible slots and show what's in them
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (_eqm.EquippedItems.TryGetValue(slot, out EquipmentSO equippedItem))
            {
                _content += $"{slot}: {equippedItem.ItemName} {GetItemStatsString(equippedItem)}\n";
            }
            else
            {
                _content += $"{slot}: None\n";
            }
        }
    }

    private string GetItemStatsString(EquipmentSO item)
    {
        string stats = "(";
        bool hasStats = false;

        void AppendStat(string name, BoundedInt stat)
        {
            // If value is set, it overrides. Otherwise it uses max (and min)
            int displayVal = stat.value != 0 ? stat.value : stat.max;
            if (displayVal != 0)
            {
                if (hasStats) stats += ", ";
                
                string sign = displayVal > 0 ? "+" : ""; // Negative numbers already have a minus sign from ToString()
                stats += $"{sign}{displayVal} {name}";
                hasStats = true;
            }
        }

        AppendStat("PHATK", item.PhysicalAttack);
        AppendStat("SPATK", item.SpecialAttack);
        AppendStat("PHDEF", item.PhysicalDefense);
        AppendStat("SPDEF", item.SpecialDefense);
        AppendStat("STR", item.STR);
        AppendStat("DEX", item.DEX);
        AppendStat("INT", item.INT);
        AppendStat("SPD", item.SPD);
        AppendStat("CRIT%", item.CritChance);
        AppendStat("BLK%", item.BlockChance);
        AppendStat("BLK DMG", item.BlockAmount);
        AppendStat("DODGE%", item.DodgeChance);

        if (hasStats) stats += ", ";
        stats += $"DUR: {item.CurrentDurability}/{item.MaxDurability}";
        hasStats = true;

        if (item.Traits != null)
        {
            foreach (var trait in item.Traits)
            {
                if (trait.Trait == EquipmentTrait.MoveFirst)
                {
                    if (hasStats) stats += ", ";
                    stats += $"CD: {trait.CurrentCooldown}";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.SoulSteal)
                {
                    if (hasStats) stats += ", ";
                    stats += "SoulSteal";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.AllStats)
                {
                    if (hasStats) stats += ", ";
                    stats += $"+{trait.Value} All Stats";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.SpellReflect)
                {
                    if (hasStats) stats += ", ";
                    stats += $"{trait.Value}% Reflect Spell";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.MaxHP)
                {
                    if (hasStats) stats += ", ";
                    stats += $"+{trait.Value}% Max HP";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.DoubleStrike)
                {
                    if (hasStats) stats += ", ";
                    stats += $"{trait.Value}% Double Strike";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.TrueStrike)
                {
                    if (hasStats) stats += ", ";
                    stats += "TrueStrike";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.ComboStrike)
                {
                    if (hasStats) stats += ", ";
                    stats += "Combo Strike";
                    hasStats = true;
                }
                else if (trait.Trait == EquipmentTrait.CounterChance)
                {
                    if (hasStats) stats += ", ";
                    stats += $"{trait.Value}% Counter Attack";
                    hasStats = true;
                }
            }
        }

        stats += ")";
        
        return hasStats ? stats : "";
    }

    private void HookUpManagers()
    {
        _rm = RunManager.Instance;
        _eqm = _rm.GetService<EquipmentManager>(); 
        
        if (_eqm != null)
        {
            _eqm.AttachObserver(this);
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EquipmentChanged)
        {
            Refresh();
        }
    }
}
