public class MidasRite : Rite
{
    public override string Description => "Enemies Drop 10% more gold";
    private readonly float k_goldMultiplier = 1.1f;

    public MidasRite() : base("Midas", 6, RiteType.Midas)
    {
        RiteType = RiteType.Midas;
    }

    public override void OnEquip(PlayerManager player)
    {
        // No implementation needed for now, effect is applied through ApplyMidasEffect method
    }

    public override void OnUnequip(PlayerManager player)
    {
        // No implementation needed for now, effect is applied through ApplyMidasEffect method
    }

    public int ApplyMidasEffect(int gold)
    {
        return (int)(gold * k_goldMultiplier); // Increase gold by 10%
    }
}
