using UnityEngine;

public enum KeyType
{
    Iron,
    Lockpick,
    Skeleton
}

[CreateAssetMenu(fileName = "New Key", menuName = "Scriptable Objects/Items/Key")]
public class KeySO : ItemSO
{
    [Header("Key Specific")]
    public KeyType Type;
    public float SuccessChance = 1.0f;
    public int MaxUses = 1;

    public KeySO()
    {
        MaxStackSize = 5;
    }
}
