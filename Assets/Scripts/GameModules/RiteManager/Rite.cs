using UnityEngine;

public abstract class Rite
{
    public string RiteID { get; protected set; }
    public int RitePointCost { get; protected set; }
    public RiteType RiteType { get; protected set; }
    public abstract string Description { get; }

    public Rite(string riteID, int ritePointCost, RiteType riteType)
    {
        RiteID = riteID;
        RitePointCost = ritePointCost;
        RiteType = riteType;
    }
    public RiteType RiteType;

    public virtual void OnEquip(PlayerManager player) { }
    public virtual void OnUnequip(PlayerManager player) { }
}
