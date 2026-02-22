public class EmpressRite : Rite
{
    public override string Description => "Gain random T2 consumable at the start of the game";

    public EmpressRite() : base("Empress", 4, RiteType.Empress)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Gain random T2 consumable at the start of the game
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
