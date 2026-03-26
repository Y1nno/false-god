using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Scriptable Objects/Items/Material")]
public class MaterialSO : ItemSO
{
    public MaterialSO()
    {
        MaxStackSize = 10;
    }
}
