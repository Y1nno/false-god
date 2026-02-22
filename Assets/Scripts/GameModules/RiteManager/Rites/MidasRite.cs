public class MidasRite : Rite
{
    public override string Description => "Enemies Drop 10% more gold";

    public MidasRite() : base("Midas", 6)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // No implementation needed for now, effect is applied through ApplyMidasEffect method
    }

    public override void OnUnequip(PlayerManager player)
    {
        // No implementation needed for now, effect is applied through ApplyMidasEffect method
    }
}
