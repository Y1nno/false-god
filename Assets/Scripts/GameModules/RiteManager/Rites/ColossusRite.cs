using UnityEngine;

public class ColossusRite : Rite
{
    private const int k_DamageBonus = 3;

    public override string Description => $"+{k_DamageBonus} Damage";

    public ColossusRite() : base("Colossus", 2) // ID: Colossus, Cost: 2
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        player.BonusDamage += k_DamageBonus;
    }

    public override void OnUnequip(PlayerManager player)
    {
        player.BonusDamage -= k_DamageBonus;
    }
}
