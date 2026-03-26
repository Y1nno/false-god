using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class SpellbookUI : MonoBehaviour, IObserver
{
    [Header("UI References")]
    public TMP_Dropdown LearnDropdown;
    public TMP_Dropdown ForgetDropdown;
    public Button LearnButton;
    public Button ForgetButton;

    private InventoryManager _inv => RunManager.Instance != null ? RunManager.Instance.GetService<InventoryManager>() : null;
    private SpellManager _sm => RunManager.Instance != null ? RunManager.Instance.GetService<SpellManager>() : null;

    private List<SpellScrollSO> _availableScrolls = new List<SpellScrollSO>();

    void Start()
    {
        RefreshUI();
    }

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_inv == null || _sm == null) return;
        UpdateLearnDropdown();
        UpdateForgetDropdown();
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.ItemAcquired || eventType == EventType.ItemRemoved || eventType == EventType.EquipmentChanged)
        {
            RefreshUI();
        }
    }

    private void UpdateLearnDropdown()
    {
        if (LearnDropdown == null || _inv == null) return;
        LearnDropdown.ClearOptions();
        _availableScrolls.Clear();

        // Find all SpellScrollInstance items in inventory
        var scrolls = _inv.UnEquippedItems
            .OfType<SpellScrollInstance>()
            .Select(s => s.BaseData)
            .ToList();

        _availableScrolls = scrolls;
        LearnDropdown.AddOptions(scrolls.Select(s => s.ItemName).ToList());
        if (LearnButton != null) LearnButton.interactable = scrolls.Count > 0;
    }

    private void UpdateForgetDropdown()
    {
        if (ForgetDropdown == null || _sm == null) return;
        ForgetDropdown.ClearOptions();
        var learned = _sm.LearnedSpells;
        if (learned != null)
        {
            ForgetDropdown.AddOptions(learned.Select(s => s.actionName).ToList());
            if (ForgetButton != null) ForgetButton.interactable = learned.Count > 0;
        }
    }

    public void OnLearnClicked()
    {
        if (LearnDropdown.options.Count == 0) return;

        int index = LearnDropdown.value;
        SpellScrollSO selectedScrollSO = _availableScrolls[index];

        if (_sm.LearnSpell(selectedScrollSO.Spell))
        {
            // Remove the scroll from inventory
            var itemToRemove = _inv.UnEquippedItems
                .OfType<SpellScrollInstance>()
                .FirstOrDefault(s => s.BaseData == selectedScrollSO);
            
            if (itemToRemove != null)
            {
                _inv.RemoveItemFromInventory(itemToRemove);
            }
            RefreshUI();
        }
    }

    public void OnForgetClicked()
    {
        if (ForgetDropdown.options.Count == 0) return;

        int index = ForgetDropdown.value;
        SpellSO selectedSpell = _sm.LearnedSpells[index];

        _sm.ForgetSpell(selectedSpell);
        RefreshUI();
    }
}
