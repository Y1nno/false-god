using UnityEngine;

[CreateAssetMenu(fileName = "Generic Item", menuName = "Scriptable Objects/Items/Generic Item")]
public class ItemSO : ScriptableObject
{
    [Header("Base Item Properties")]
    public string ItemName;
    public Sprite ItemIcon;
    public string Description;
}
