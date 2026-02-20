public class PalamedesRite : Rite
{
    public override string Description => "Reroll Dice";

    public PalamedesRite() : base("Palamedes", 3)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Reroll Dice
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
