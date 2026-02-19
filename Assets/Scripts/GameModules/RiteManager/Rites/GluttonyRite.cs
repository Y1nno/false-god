public class GluttonyRite : Rite
{
    public override string Description => "Consumables are 20% more effective";

    public GluttonyRite() : base("Gluttony", 2)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Consumables are 20% more effective
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
