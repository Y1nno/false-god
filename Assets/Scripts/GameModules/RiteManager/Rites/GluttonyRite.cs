public class GluttonyRite : Rite
{
    public override string Description => "Consumables are 20% more effective";
    private readonly float k_consumableEffectivenessMultiplier = 1.2f;
    public float Multiplier => k_consumableEffectivenessMultiplier;

    public GluttonyRite() : base("Gluttony", 2)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // Gluttony Effect applied just by being active, the calculations for consumable effectiveness check for the rite, so no additional setup needed
    }

    public override void OnUnequip(PlayerManager player)
    {
        // No need to reset anything on unequip since the effect is only checked when calculating consumable effectiveness
    }
}
