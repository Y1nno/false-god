using UnityEngine;

public abstract class Rite
{
    public string RiteID { get; protected set; }
    public int RitePointCost { get; protected set; }
    public abstract string Description { get; }

    public Rite(string riteID, int ritePointCost)
    {
        RiteID = riteID;
        RitePointCost = ritePointCost;
    }

    public virtual void OnEquip(PlayerManager player) { }
    public virtual void OnUnequip(PlayerManager player) { }
}
