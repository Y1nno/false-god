public class MerlinRite : Rite
{
    public override string Description => "Special atk increased 10% but - 10% Def";

    public MerlinRite() : base("Merlin", 5)
    {
        RiteType = RiteType.Merlin;
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Special atk increased 10% but - 10% Def
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
