using UnityEngine;
using TMPro;
using System;

public class InputInterface : MonoBehaviour
{
    
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

}
