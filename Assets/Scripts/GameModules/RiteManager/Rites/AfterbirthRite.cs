using UnityEngine;

public class AfterbirthRite : Rite
{
    public override string Description => "Afterbirth (Choose 1 relic to carry over to your next run upon death)";

    public AfterbirthRite() : base("Afterbirth", 4, RiteType.Afterbirth)
    {
    }
}
