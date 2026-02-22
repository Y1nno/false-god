public class JuggernautRite : Rite
{
    public override string Description => "Cannot be stunned";

    public JuggernautRite() : base("Juggernaut", 4, RiteType.Juggernaut)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // TODO: Cannot be stunned
    }

    public override void OnUnequip(PlayerManager player)
    {
        // TODO: Remove effect
    }
}
