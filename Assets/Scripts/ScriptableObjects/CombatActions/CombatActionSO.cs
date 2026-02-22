using UnityEngine;

[CreateAssetMenu(fileName = "CombatAction", menuName = "Scriptable Objects/Combat/CombatAction")]
public class CombatActionSO : ScriptableObject
{
    public string actionName;
    public int power;
    public int cost;
    public Sprite icon;
}
