using UnityEngine;
using TMPro;

public class InfoContainer : MonoBehaviour
{
    private string _content;

    private RunManager _rm = null;
    private PlayerManager _pm = null;
    private EconomyManager _em = null;
    private EncounterManager _enm = null;

    public TMP_Text textBox;

    public void Refresh()
    {
        if (_rm == null)
        {
            HookUpManagers();
        }
        
        AddPlayerInfo();
        AddEconomyInfo();
        textBox.text = _content;
    }

    private void AddPlayerInfo()
    {
        _content = "Player Info:\n";
        _content += "Health: " + _pm.Health.CurrentValue + "/" + _pm.Health.MaxValue + "\n";
        _content += "Mana: " + _pm.Mana.CurrentValue + "/" + _pm.Mana.MaxValue + "\n";
        
    }

    private void AddEconomyInfo()
    {
        _content += "Gold: " + _em.GetCurrentGold() + "\n";
    }

    private void HookUpManagers()
    {
        _rm = RunManager.Instance;
        _pm = _rm.GetService<PlayerManager>();
        _em = _rm.GetService<EconomyManager>();
        _enm = _rm.GetService<EncounterManager>();
    }
}
