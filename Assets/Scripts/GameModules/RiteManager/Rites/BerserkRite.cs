public class BerserkRite : Rite
{
    public override string Description => "Physical damge increased 10%, but -10% Sp.Def";
    private readonly float k_physicalDamageMultiplier = 0.1f;
    private readonly float k_specialDefenseMultiplier = -0.1f;

    public float PhysicalAttackMultiplier => k_physicalDamageMultiplier;
    public float SpecialDefenseMultiplier => k_specialDefenseMultiplier;

    public BerserkRite() : base("Berserk", 5, RiteType.Berserk)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // Beserk Effect applied just by being active, the calculations for secondary stats check for the rite, so no additional setup needed
    }

    public override void OnUnequip(PlayerManager player)
    {
        // Berserk Effect removed just by being inactive, no additional teardown needed
    }
}
