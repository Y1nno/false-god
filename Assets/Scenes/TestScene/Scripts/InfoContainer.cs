using UnityEngine;
using TMPro;

public class InfoContainer : MonoBehaviour
{
    private string _content;

    private RunManager _rm = null;

    public TMP_Text textBox;

    public void Refresh()
    {
        if (_rm == null)
        {
            _rm = RunManager.Instance;
        }
        _content = "Player Info:\n";
        AddPlayerInfo();
        textBox.text = _content;
    }

    private void AddPlayerInfo()
    {
        PlayerManager pm = _rm.GetService<PlayerManager>();
        _content += "Health: " + pm.Health.CurrentValue + "/" + pm.Health.MaxValue + "\n";
        _content += "Mana: " + pm.Mana.CurrentValue + "/" + pm.Mana.MaxValue + "\n";
    }
}
