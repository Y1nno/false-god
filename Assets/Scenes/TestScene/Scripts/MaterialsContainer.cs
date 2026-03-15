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
        _content = "Materials Inventory:\n\n";

        AddMaterialsInfo();

        _content += "\nRelics Inventory:\n\n";
        AddRelicsInfo();

        textBox.text = _content;
    }

    private void AddMaterialsInfo()
    {
        if (_invm == null)
        {
            _content += "No Inventory Manager connected.\n";
            return;
        }

        bool hasMaterials = false;
        foreach (var item in _invm.UnEquippedItems)
        {
            if (item is MaterialInstance mat)
            {
                _content += $"- {mat.BaseData.ItemName} x{mat.Quantity} ({mat.BaseData.SellPrice} Gold)\n";
                hasMaterials = true;
            }
        }

        if (!hasMaterials)
        {
            _content += "Empty\n";
        }
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

        if (!hasRelics)
        {
            _content += "None\n";
        }
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
