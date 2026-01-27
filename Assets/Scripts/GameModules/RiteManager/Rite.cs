using UnityEngine;

public class Rite
{
    public string RiteID { get; private set; }
    public int RitePointCost { get; private set; }

    public Rite(string riteID, int ritePointCost)
    {
        RiteID = riteID;
        RitePointCost = ritePointCost;
    }
}
