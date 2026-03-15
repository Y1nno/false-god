using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Scriptable Objects/Items/Material")]
public class MaterialSO : ItemSO
{
    [Header("Material Specific")]
    public int StackSize = 99; // Optional: for future stacking logic
}
