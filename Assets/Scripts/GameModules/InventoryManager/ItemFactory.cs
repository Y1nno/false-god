using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ItemFactory
{
    public static Item CreateItemByID(string itemID, int tier = 1)
    {
        ItemSO so = FindItemSO(itemID);
        if (so == null)
        {
            Debug.LogError($"ItemFactory: Could not find ItemSO for ID '{itemID}'.");
            return null;
        }

        if (so is EquipmentSO eqSO)
        {
            return new Equipment(eqSO.InstantiateAndRollStats());
        }
        else if (so is Consumable conSO)
        {
            return new ConsumableInstance(conSO, tier);
        }
        else if (so is MaterialSO matSO)
        {
            return new MaterialInstance(matSO);
        }
        else if (so is KeySO keySO)
        {
            return new KeyInstance(keySO);
        }
        else if (so is RelicSO relicSO)
        {
            return new RelicInstance(relicSO);
        }

        return null;
    }

    private static ItemSO FindItemSO(string itemID)
    {
        ItemSO found = null;
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:ItemSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSO so = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (so != null && (so.ItemID == itemID || so.name == itemID || so.name == itemID + "SO"))
            {
                found = so;
                break;
            }
        }
#else
        // Fallback for builds - requires items to be in a Resources folder
        found = Resources.Load<ItemSO>(itemID) ?? Resources.Load<ItemSO>(itemID + "SO");
#endif
        return found;
    }
}
