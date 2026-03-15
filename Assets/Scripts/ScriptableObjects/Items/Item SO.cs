using UnityEngine;

[CreateAssetMenu(fileName = "Generic Item", menuName = "Scriptable Objects/Items/Generic Item")]
public class ItemSO : ScriptableObject
{
    [Header("Base Item Properties")]
    [Tooltip("The unique ID used to look up this item in code. Do not change this once set.")]
    public string ItemID;
    public string ItemName;
    public Sprite ItemIcon;
    public string Description;
    public int SellPrice;
}
