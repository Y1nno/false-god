public class BerserkRite : Rite
{
    public override string Description => "Physical damge increased 10%, but -10% Sp.Def";

    public BerserkRite() : base("Berserk", 5)
    {
        RiteType = RiteType.Berserk;
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Physical damge increased 10%, but -10% Sp.Def
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
