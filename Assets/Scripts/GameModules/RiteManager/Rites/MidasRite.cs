public class MidasRite : Rite
{
    public override string Description => "Enemies Drop 10% more gold";
    private readonly float k_goldMultiplier = 1.1f;

    public MidasRite() : base("Midas", 6)
    {
        RiteType = RiteType.Midas;
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Enemies Drop 10% more gold
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }

    public int ApplyMidasEffect(int gold)
    {
        return (int)(gold * k_goldMultiplier); // Increase gold by 10%
    }
}
