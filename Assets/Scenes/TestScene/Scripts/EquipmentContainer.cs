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
            if (_eqm.EquippedItems.TryGetValue(slot, out Equipment equippedItem))
            {
                _content += $"{slot}: {equippedItem.GetName()} {GetItemStatsString(equippedItem)}\n";
            }
            else
            {
                _content += $"{slot}: None\n";
            }
        }
    }

    private string GetItemStatsString(Equipment item)
    {
        string stats = "(";
        bool hasStats = false;

        void AppendStat(string name, int displayVal)
        {
            if (displayVal != 0)
            {
                if (hasStats) stats += ", ";
                
                string sign = displayVal > 0 ? "+" : ""; 
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
        AppendStat("HP", item.MaxHP);
        AppendStat("MP", item.MaxMana);

        if (hasStats) stats += ", ";
        stats += $"DUR: {item.CurrentDurability}/{item.MaxDurability}";
        hasStats = true;

        if (item.Traits != null)
        {
            foreach (var trait in item.Traits)
            {
                if (hasStats) stats += ", ";
                
                switch (trait.Trait)
                {
                    case EquipmentTrait.MoveFirst:
                        stats += $"CD: {trait.CurrentCooldown}";
                        break;
                    case EquipmentTrait.SoulSteal:
                        stats += "SoulSteal";
                        break;
                    case EquipmentTrait.AllStats:
                        stats += $"+{trait.Value} All Stats";
                        break;
                    case EquipmentTrait.SpellReflect:
                        stats += $"{trait.Value}% Reflect Spell";
                        break;
                    case EquipmentTrait.MaxHP:
                        stats += $"+{trait.Value}% Max HP";
                        break;
                    case EquipmentTrait.DoubleStrike:
                        stats += $"{trait.Value}% Double Strike";
                        break;
                    case EquipmentTrait.TrueStrike:
                        stats += "TrueStrike";
                        break;
                    case EquipmentTrait.ComboStrike:
                        stats += "Combo Strike";
                        break;
                    case EquipmentTrait.CounterChance:
                        stats += $"{trait.Value}% Counter Attack";
                        break;
                    case EquipmentTrait.PoisonHit:
                        stats += $"{trait.Value}% Poison";
                        break;
                    case EquipmentTrait.BurnHit:
                        stats += $"{trait.Value}% Burn";
                        break;
                    case EquipmentTrait.FreezeHit:
                        stats += $"{trait.Value}% Freeze";
                        break;
                    case EquipmentTrait.WeakenHit:
                        stats += $"{trait.Value}% Weaken";
                        break;
                    case EquipmentTrait.HealthRegen:
                        stats += $"+{trait.Value} Regen";
                        break;
                    case EquipmentTrait.ManaRegen:
                        stats += $"+{trait.Value} Mana Regen";
                        break;
                    case EquipmentTrait.DamageReflect:
                        stats += $"{trait.Value}% Reflect";
                        break;
                    default:
                        stats += trait.Trait.ToString();
                        break;
                }
                hasStats = true;
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
