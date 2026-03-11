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

    public void RefreshUI()
    {
        InfoContainer info = GameObject.FindAnyObjectByType<InfoContainer>();
        if (info != null) info.Refresh();

        EquipmentContainer eq = GameObject.FindAnyObjectByType<EquipmentContainer>();
        if (eq != null) eq.Refresh();
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

    public void UseConsumableFromDropdown(UnityEngine.GameObject panel)
    {
        if (panel == null) return;
        
        // Find the dropdown in the panel
        Transform dropdownTransform = panel.transform.Find("ConsumableDropdown") ?? panel.transform; 
        TMP_Dropdown dropdown = dropdownTransform.GetComponent<TMP_Dropdown>();
        
        // If still null, search globally just in case
        if (dropdown == null)
        {
            GameObject globalDropdown = GameObject.Find("ConsumableDropdown");
            if (globalDropdown != null) dropdown = globalDropdown.GetComponent<TMP_Dropdown>();
        }
        
        if (dropdown == null || dropdown.options.Count == 0) return;

        string selectedItemName = dropdown.options[dropdown.value].text;
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        
        if (invm == null) return;

        // Find the corresponding consumable in the player's unequipped inventory list
        Item itemToHandle = invm.UnEquippedItems.Find(item => 
        {
            if (item is ConsumableInstance con) return con.BaseData.ItemName == selectedItemName;
            return false;
        });

        if (itemToHandle != null)
        {
            invm.UseOrEquipItem(itemToHandle);
            
            // Re-populate the dropdowns since the inventory changed
            RefreshAllDropdowns(panel);
            
            // Refresh the equipment panel UI
            EquipmentContainer eqUI = GameObject.FindAnyObjectByType<EquipmentContainer>();
            if (eqUI != null) eqUI.Refresh();
        }
        else
        {
             TextOutputter.Instance.OutputText($"Could not find '{selectedItemName}' in your Consumables Inventory.");
        }
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

    // --- Equipment & Inventory Testing UI ---

    public void AddEquipmentToInventory(UnityEngine.GameObject txtInput)
    {
        if (txtInput == null) return;
        TMP_InputField inputField = txtInput.GetComponent<TMP_InputField>();
        string itemID = inputField.text.Trim();

        int tier = 1;
        // Attempt to blindly locate the floating Tier field to snag its multiplier before generating
        GameObject tierObj = GameObject.Find("TierInputField");
        if (tierObj != null)
        {
            TMP_InputField tierField = tierObj.GetComponent<TMP_InputField>();
            if (tierField != null && int.TryParse(tierField.text, out int parsedTier))
            {
                tier = parsedTier;
            }
        }

        EquipmentSO newEquipment = null;
        Consumable newConsumable = null;
        
#if UNITY_EDITOR
        // If not using a Resources folder, we must search the AssetDatabase in the editor
        string[] eqGuids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentSO");
        foreach(string guid in eqGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EquipmentSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentSO>(path);
            
            // Check if either the filename matches OR the new ItemID field matches exactly
            if (so != null && (so.ItemID == itemID || so.name == itemID || so.name == itemID + "SO"))
            {
                newEquipment = so;
                break;
            }
        }
        
        string[] conGuids = UnityEditor.AssetDatabase.FindAssets("t:Consumable");
        foreach(string guid in conGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Consumable so = UnityEditor.AssetDatabase.LoadAssetAtPath<Consumable>(path);
            
            if (so != null && (so.ItemID == itemID || so.name == itemID || so.name == itemID + "SO"))
            {
                newConsumable = so;
                break;
            }
        }
#else
        // If testing in a build later, they MUST be in Resources or an Addressables group.
        newEquipment = Resources.Load<EquipmentSO>(itemID) ?? Resources.Load<EquipmentSO>(itemID + "SO");
        newConsumable = Resources.Load<Consumable>(itemID) ?? Resources.Load<Consumable>(itemID + "SO");
#endif

        if (newEquipment != null)
        {
            // Give the player a uniquely rolled instance of the equipment
            EquipmentSO rolledInstance = newEquipment.InstantiateAndRollStats();
            
            // Wrap the data inside an unequipped runtime model so the UI can cast it successfully later
            Equipment weaponInstance = new Equipment(rolledInstance);
            
            RunManager.Instance.GetService<InventoryManager>()?.AddItemToInventory(weaponInstance);
            RefreshAllDropdowns();
        }
        else if (newConsumable != null)
        {
            // Successfully fetched the consumable asset. Wrap it in an instance to lock in its Tier!
            ConsumableInstance potionInstance = new ConsumableInstance(newConsumable, tier);
            
            RunManager.Instance.GetService<InventoryManager>()?.AddItemToInventory(potionInstance);
            RefreshAllDropdowns();
        }
        else
        {
            TextOutputter.Instance.OutputText($"Could not find Equipment or Consumable with ID/filename '{itemID}' in the project.");
        }
    }

    public void EquipItemFromDropdown(UnityEngine.GameObject panel)
    {
        if (panel == null) return;
        
        // Find the dropdown in the panel
        Transform dropdownTransform = panel.transform.Find("EquipmentDropdown") ?? panel.transform; // Fallback to itself if it IS the dropdown
        TMP_Dropdown dropdown = dropdownTransform.GetComponent<TMP_Dropdown>();
        
        // If still null, search globally just in case
        if (dropdown == null)
        {
            GameObject globalDropdown = GameObject.Find("EquipmentDropdown");
            if (globalDropdown != null) dropdown = globalDropdown.GetComponent<TMP_Dropdown>();
        }
        
        if (dropdown == null || dropdown.options.Count == 0) return;

        string selectedItemName = dropdown.options[dropdown.value].text;
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        
        if (invm == null) return;

        // Find the corresponding item in the player's unequipped inventory list
        // Note: we're matching by ItemName here because the Dropdown displays names, not IDs.
        Item itemToHandle = invm.UnEquippedItems.Find(item => 
        {
            if (item is Equipment eq) return eq.ItemName == selectedItemName;
            if (item is ConsumableInstance con) return con.BaseData.ItemName == selectedItemName;
            return false;
        });

        if (itemToHandle != null)
        {
            invm.UseOrEquipItem(itemToHandle);
            
            // Re-populate the dropdowns since the inventory changed
            RefreshAllDropdowns(panel);
            
            // Refresh the equipment panel UI
            EquipmentContainer eqUI = GameObject.FindAnyObjectByType<EquipmentContainer>();
            if (eqUI != null) eqUI.Refresh();
        }
        else
        {
             TextOutputter.Instance.OutputText($"Could not find '{selectedItemName}' in your Inventory.");
        }
    }

    // Helper to keep both dropdowns synchronized with the InventoryManager unified list
    public void RefreshAllDropdowns(UnityEngine.GameObject panel = null)
    {
         TMP_Dropdown equipDropdown = null;
         TMP_Dropdown consumableDropdown = null;
         
         if (panel != null)
         {
             Transform eTransform = panel.transform.Find("EquipmentDropdown");
             Transform cTransform = panel.transform.Find("ConsumableDropdown");
             if (eTransform != null) equipDropdown = eTransform.GetComponent<TMP_Dropdown>();
             if (cTransform != null) consumableDropdown = cTransform.GetComponent<TMP_Dropdown>();
         }

         // Fallback to global search if panel search failed or no panel was provided
         if (equipDropdown == null)
         {
             GameObject globalEDropdown = GameObject.Find("EquipmentDropdown");
             if (globalEDropdown != null) equipDropdown = globalEDropdown.GetComponent<TMP_Dropdown>();
         }
         
         if (consumableDropdown == null)
         {
             GameObject globalCDropdown = GameObject.Find("ConsumableDropdown");
             if (globalCDropdown != null) consumableDropdown = globalCDropdown.GetComponent<TMP_Dropdown>();
         }
         
         InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
         if (invm == null) return;
        
         string consumablesText = "Consumables Inventory:\n\n";
         string equipmentText = "Equipment Inventory:\n\n";
         // TODO: Add Upgrade Materials inventory block here in the future
         
         List<string> equipOptions = new List<string>();
         List<string> consumableOptions = new List<string>();

         if (invm.UnEquippedItems.Count == 0)
         {
             consumablesText += "Empty";
             equipmentText += "Empty";
         }
         else
         {
             bool hasConsumables = false;
             bool hasEquipment = false;
             
             foreach (var item in invm.UnEquippedItems)
             {
                 if (item is ConsumableInstance con)
                 {
                     consumablesText += $"- {con.BaseData.ItemName} (Tier {con.Tier})\n";
                     consumableOptions.Add(con.BaseData.ItemName); // We display Name in UI
                     hasConsumables = true;
                 }
                 else if (item is Equipment eq)
                 {
                     equipmentText += $"- {eq.ItemName}\n";
                     equipOptions.Add(eq.ItemName); // We display Name in UI
                     hasEquipment = true;
                 }
             }
             
             if (!hasConsumables) consumablesText += "Empty";
             if (!hasEquipment) equipmentText += "Empty";
         }
         
         // Only print both splits back to the console box (or visual text box) if needed
         TextOutputter.Instance.OutputText(consumablesText + "\n\n" + equipmentText);

         // Populate Equipment UI Dropdown
         if (equipDropdown != null)
         {
             equipDropdown.ClearOptions();
             equipDropdown.AddOptions(equipOptions);
         }
         
         // Populate Consumable UI Dropdown
         if (consumableDropdown != null)
         {
             consumableDropdown.ClearOptions();
             consumableDropdown.AddOptions(consumableOptions);
         }
    }
}
