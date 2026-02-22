public class GluttonyRite : Rite
{
    public override string Description => "Consumables are 20% more effective";

    public GluttonyRite() : base("Gluttony", 2)
    {
        RiteType = RiteType.Gluttony;
    }

    public override void OnEquip(PlayerManager player)
    {
        if (player.PlayerStats is PlayerStatBox psb)
        {
            psb.ConsumableEffectivenessMultiplier = 1.2f;
        }
    }

    public override void OnUnequip(PlayerManager player)
    {
        if (player.PlayerStats is PlayerStatBox psb)
        {
            psb.ConsumableEffectivenessMultiplier = 1.0f;
        }
    }
}
