public class PalamedesRite : Rite
{
    public override string Description => "Reroll Dice";
    private readonly int k_RerollCount = 1;

    public PalamedesRite() : base("Palamedes", 3)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        player.DiceRerollCount += k_RerollCount;
    }

    public override void OnUnequip(PlayerManager player)
    {
        player.DiceRerollCount -= k_RerollCount;
    }
}
