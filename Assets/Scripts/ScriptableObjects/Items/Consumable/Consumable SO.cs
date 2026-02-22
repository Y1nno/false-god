using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Items/Consumable")]
public class ConsumableSO : ItemSO
{
    [Header("Consumable Specific")]
    public int HealthRecoverAmount;
    public int ManaRecoverAmount;
}
