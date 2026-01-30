using UnityEngine;
using TMPro;

public class InfoContainer : MonoBehaviour
{
    private string _content;

    private RunManager _rm = null;
    private PlayerManager _pm = null;
    private EconomyManager _em = null;
    private EncounterManager _enm = null;
    private XPManager _exm = null;

    private ScoreManager _sm = null;
    private RiteManager _ritem = null;
    private ReligionManager _relm = null;

    public TMP_Text textBox;

    public void Refresh()
    {
        if (_rm == null)
        {
            HookUpManagers();
        }
        _content = "";

        AddScoreInfo();
        AddPlayerInfo();
        AddStatInfo();
        AddEconomyInfo();
        AddXPInfo();
        AddReligionInfo();
        textBox.text = _content;
    }

    private void AddScoreInfo()
    {
        _content += "Current Score: " + _sm.CurrentScore + "\n";
        _content += "Rite Points: " + _rm.GetService<RiteManager>().CalculateRitePointsRemaining() + "/" + _rm.GetService<RiteManager>().RitePoints + "\n";
    }

    private void AddPlayerInfo()
    {
        _content += "Player Info:\n";
        _content += "Health: " + _pm.Health.CurrentValue + "/" + _pm.Health.MaxValue + "\n";
        _content += "Mana: " + _pm.Mana.CurrentValue + "/" + _pm.Mana.MaxValue + "\n";
    }

    private void AddStatInfo()
    {
        _content += "Stats:\n";
        _content += "STR: " + _pm.PlayerStats.STR + "\n";
        _content += "DEX: " + _pm.PlayerStats.DEX + "\n";
        _content += "INT: " + _pm.PlayerStats.INT + "\n";
        _content += "LCK: " + _pm.PlayerStats.LCK + "\n";
    }

    private void AddEconomyInfo()
    {
        _content += "Gold: " + _em.GetCurrentGold() + "\n";
    }

    private void AddXPInfo()
    {
        _content += "XP: " + _exm.currentXP + "/" + _exm.xpThresholdForLevelUp + "\n";
        _content += "Level: " + _exm.level + "\n";
        _content += "Stat points: " + _pm.PlayerStats.AvailableStatPoints + "\n";
    }

    private void AddReligionInfo()
    {
        if (_relm.CurrentReligion != null)
        {
            _content += "Religion: " + _relm.CurrentReligion.ReligionID + "\n";
        }
        else
        {
            _content += "No current religion.\n";
        }
    }

    private void HookUpManagers()
    {
        _rm = RunManager.Instance;
        _pm = _rm.GetService<PlayerManager>();
        _em = _rm.GetService<EconomyManager>();
        _enm = _rm.GetService<EncounterManager>();
        _exm = _rm.GetService<XPManager>();
        _sm = _rm.GetService<ScoreManager>();
        _ritem = _rm.GetService<RiteManager>();
        _relm = _rm.GetService<ReligionManager>();
    }
}
