using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class InputInterface : MonoBehaviour
{
    public Prompt activePrompt = null;
    public void StartNewRun()
    {
        RunManager.Instance.StartNewRun();
    }

    public void AdvanceDungeonRoom()
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        dm.AdvanceRoom();
    }

    public void ResolveCurrentEncounter()
    {
        EncounterManager em = RunManager.Instance.GetService<EncounterManager>();
        em.ResolveEncounter();
    }

    public void AddGold(UnityEngine.GameObject txtInput)
    {
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (txtInput == null) return;
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        int amount = int.Parse(inputField.text);
        em.AddGold(amount);
    }

    public void SpendGold(UnityEngine.GameObject txtInput)
    {
        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (txtInput == null) return;
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        int amount = int.Parse(inputField.text);
        em.SpendGold(amount);
    }

    public void GainXP(UnityEngine.GameObject txtInput)
    {
        XPManager xm = RunManager.Instance.GetService<XPManager>();
        if (txtInput == null) return;
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        int amount = int.Parse(inputField.text);
        xm.AddXP(amount);
    }

    public void SpendStatPoints(UnityEngine.GameObject panel)
    {
        Transform txtStatInput = panel.transform.Find("StatDropdown");
        Transform txtPointsInput = panel.transform.Find("PointsInputField");
        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        if (txtStatInput == null || txtPointsInput == null) return;
        TMP_Dropdown statInputField = txtStatInput.GetComponent<TMP_Dropdown>();
        TMP_InputField pointsInputField = txtPointsInput.GetComponent<TMP_InputField>();
        if (!Enum.TryParse(statInputField.captionText.text, out Stat stat)) return;
        int points = int.Parse(pointsInputField.text);
        bool success = pm.PlayerStats.SpendStatPoints(stat, points);
        if (!success)
        {
            TextOutputter.Instance.OutputText("Not enough stat points to spend.");
        }
    }

    public void MakeEncounterDecision(UnityEngine.GameObject txtInput)
    {
        if (txtInput == null) return;
        EncounterManager em = RunManager.Instance.GetService<EncounterManager>();
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        int decisionIndex = int.Parse(inputField.text) -1; // Convert to zero-based index
        activePrompt.RecieveDecision(decisionIndex);
    }

    public void AddRite(string riteName)
    {
        if (Enum.TryParse(riteName, true, out RiteType type))
        {
            RunManager.Instance.GetService<RiteManager>().EquipRite(type);
            RefreshUI();
        }
        else
        {
            TextOutputter.Instance.OutputText($"Invalid Rite Name: {riteName}");
            Debug.LogError($"Could not parse Enum: {riteName}");
        }
    }

    public void AddRitePoints(UnityEngine.GameObject txtInput)
    {
        RiteManager rm = RunManager.Instance.GetService<RiteManager>();
        if (txtInput == null) return;
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        if (int.TryParse(inputField.text, out int amount))
        {
            rm.AddBaseRitePoints(amount);
            RefreshUI();
        }
        else
        {
             Debug.LogWarning($"Could not parse int from input field: {inputField.text}");
        }
    }

    public void RollForStat(UnityEngine.GameObject panel)
    {
        Transform txtStatInput = panel.transform.Find("StatDropdown");
        Transform txtThresholdInput = panel.transform.Find("Threshold");
        if (txtStatInput == null || txtThresholdInput == null) return;
        TMP_Dropdown statInputField = txtStatInput.GetComponent<TMP_Dropdown>();
        TMP_InputField thresholdInputField = txtThresholdInput.GetComponent<TMP_InputField>();
        if (!Enum.TryParse(statInputField.captionText.text, out Stat stat)) return;
        int threshold = int.Parse(thresholdInputField.text);
        //Debug.Log($"Rolling for stat: {stat} with threshold: {threshold}");
        DiceRoller.Instance.RollForStat(stat, threshold);
    }

    private void RefreshUI()
    {
        InfoContainer info = GameObject.FindAnyObjectByType<InfoContainer>();
        if (info != null) info.Refresh();
    }

    // --- Improved UI Logic: Direct Read ---
    // These methods read the dropdown value at the moment of the click, preventing cross-talk between multiple dropdowns.

    public void AddRiteFromDropdown(UnityEngine.GameObject dropdownObj)
    {
        RiteType type = ParseRiteFromDropdown(dropdownObj);
        RunManager.Instance.GetService<RiteManager>().EquipRite(type);
        RefreshUI();
    }

    public void UnequipRiteFromDropdown(UnityEngine.GameObject dropdownObj)
    {
        RiteType type = ParseRiteFromDropdown(dropdownObj);
        RunManager.Instance.GetService<RiteManager>().UnequipRite(type);
        RefreshUI();
    }

    private RiteType ParseRiteFromDropdown(UnityEngine.GameObject dropdownObj)
    {
        if (dropdownObj == null)
        {
            Debug.LogError("Dropdown GameObject passed to InputInterface is null.");
            return RiteType.Colossus; // Default safe fall back
        }

        TMP_Dropdown dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogError("GameObject passed does not have a TMP_Dropdown component.");
            return RiteType.Colossus;
        }

        string selectedOption = dropdown.options[dropdown.value].text;
        Debug.Log($"Direct Read from {dropdownObj.name}: {selectedOption}");

        if (Enum.TryParse(selectedOption, true, out RiteType type))
        {
            return type;
        }
        else
        {
            Debug.LogWarning($"Could not parse Rite Enum from string: {selectedOption}");
            return RiteType.Colossus;
        }
    }
    
    
    // --- Legacy / Shared State Logic (Deprecated but kept to avoid breaking existing link immediately) ---
    private RiteType _selectedRite = RiteType.Colossus;

    public void SelectRite(UnityEngine.GameObject txtInput)
    {
        _selectedRite = ParseRiteFromDropdown(txtInput);
    }

    public void AddSelectedRite()
    {
        RunManager.Instance.GetService<RiteManager>().EquipRite(_selectedRite);
        RefreshUI();
    }

    public void UnequipSelectedRite()
    {
        RunManager.Instance.GetService<RiteManager>().UnequipRite(_selectedRite); 
        RefreshUI();
    }
}
