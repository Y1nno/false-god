public class MerlinRite : Rite
{
    public override string Description => "Special atk increased 10% but - 10% Def";
    private readonly float specialAttackMultiplier = 0.1f;
    private readonly float physicalDefenseMultiplier = -0.1f;

    public float SpecialAttackMultiplier => specialAttackMultiplier;
    public float PhysicalDefenseMultiplier => physicalDefenseMultiplier;

    public MerlinRite() : base("Merlin", 5, RiteType.Merlin)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        // Merlin Effect applied just by being active, the calculations for secondary stats check for the rite, so no additional setup needed
    }

    public override void OnUnequip(PlayerManager player)
    {
        // Merlin Effect removed just by being inactive, no additional teardown needed
    }
}
