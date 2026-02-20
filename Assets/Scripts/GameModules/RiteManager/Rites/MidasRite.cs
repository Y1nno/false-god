public class MidasRite : Rite
{
    public override string Description => "Enemies Drop 10% more gold";

    public MidasRite() : base("Midas", 6)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Enemies Drop 10% more gold
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
