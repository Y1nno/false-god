using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MaterialsContainer : MonoBehaviour, IObserver
{
    private string _content;
    private RunManager _rm = null;
    private InventoryManager _invm = null;

    public TMP_Text textBox;

    public void Refresh()
    {
        if (_rm == null)
        {
            HookUpManagers();
        }
        
        _content = "<size=120%><color=#FFA500>UNIFIED INVENTORY</color></size>\n\n";
        
        AddConsumablesInfo();
        AddEquipmentInfo();
        AddMaterialsInfo();
        AddKeysInfo();
        AddRelicsInfo();

        textBox.text = _content;
    }

    private void AddConsumablesInfo()
    {
        _content += "<b>Consumables:</b>\n";
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is ConsumableInstance con)
            {
                string nameWithTier = $"{con.GetName()} (Tier {con.Tier})";
                counts[nameWithTier] = counts.GetValueOrDefault(nameWithTier) + 1;
            }
        }
        AppendGroupedInfo(counts);
    }

    private void AddEquipmentInfo()
    {
        _content += "<b>Equipment:</b>\n";
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is Equipment eq)
            {
                string name = eq.GetName();
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }
        }
        AppendGroupedInfo(counts);
    }

    private void AddMaterialsInfo()
    {
        _content += "<b>Materials:</b>\n";
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is MaterialInstance mat)
            {
                string name = mat.GetName();
                counts[name] = counts.GetValueOrDefault(name) + mat.Quantity;
            }
        }
        AppendGroupedInfo(counts);
    }

    private void AddKeysInfo()
    {
        _content += "<b>Keys:</b>\n";
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is KeyInstance key)
            {
                string name = key.GetName();
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }
        }
        AppendGroupedInfo(counts);
    }

    private void AddRelicsInfo()
    {
        _content += "<b>Relics:</b>\n";
        bool hasRelics = false;
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is RelicInstance relic)
            {
                _content += $"- {relic.GetName()}\n";
                hasRelics = true;
            }
        }
        if (!hasRelics) _content += "None\n";
        _content += "\n";
    }

    private void AppendGroupedInfo(Dictionary<string, int> counts)
    {
        if (counts.Count == 0)
        {
            _content += "None\n";
        }
        else
        {
            foreach (var pair in counts)
            {
                _content += $"- {pair.Key} x{pair.Value}\n";
            }
        }
        _content += "\n";
    }

    private void HookUpManagers()
    {
        _rm = RunManager.Instance;
        _invm = _rm.GetService<InventoryManager>(); 
        
        if (_invm != null)
        {
            _invm.AttachObserver(this);
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.ItemAcquired)
        {
            Refresh();
        }
    }
}
