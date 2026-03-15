using UnityEngine;
using TMPro;

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
        
        _content = "Materials Inventory:\n";
        AddMaterialsInfo();

        _content += "\nKeys Inventory:\n";
        AddKeysInfo();

        _content += "\nRelics Inventory:\n";
        AddRelicsInfo();

        textBox.text = _content;
    }

    private void AddMaterialsInfo()
    {
        if (_invm == null) return;

        bool hasMaterials = false;
        // Since Materials are now consolidated in InventoryManager, we just list them
        // but we'll group them here too just in case or if multiple stacks are allowed later.
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is MaterialInstance mat)
            {
                _content += $"- {mat.BaseData.ItemName} x{mat.Quantity} ({mat.BaseData.SellPrice} Gold)\n";
                hasMaterials = true;
            }
        }

        if (!hasMaterials) _content += "None\n";
    }

    private void AddKeysInfo()
    {
        if (_invm == null) return;

        bool hasKeys = false;
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is KeyInstance key)
            {
                _content += $"- {key.BaseData.ItemName} ({key.RemainingUses} Uses)\n";
                hasKeys = true;
            }
        }

        if (!hasKeys) _content += "None\n";
    }

    private void AddRelicsInfo()
    {
        if (_invm == null) return;

        bool hasRelics = false;
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is RelicInstance relic)
            {
                _content += $"- {relic.BaseData.ItemName}\n";
                hasRelics = true;
            }
        }

        if (!hasRelics) _content += "None\n";
    }

    private void HookUpManagers()
    {
        _rm = RunManager.Instance;
        _invm = _rm.GetService<InventoryManager>(); 
        
        // Note: InventoryManager doesn't currently notify on change, 
        // but it's good practice to attach for future logic.
        // For now, InputInterface manual refreshes it.
    }

    public void OnNotify(object subject, EventType eventType)
    {
        // Add listeners here if InventoryManager/DropManager start broadcasting specific events
    }
}
