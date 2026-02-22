using UnityEngine;

public class OuroborosRite : Rite
{
    public override string Description => "Ouroboros (HP as Mana)";

    public OuroborosRite() : base("Ouroboros", 3, RiteType.Ouroboros) // ID: Ouroboros, Cost: 3
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        player.CanUseHealthAsMana = true;
        TextOutputter.Instance.OutputText("Ouroboros Rite: The cycle begins. Health will fuel your magic.");
    }

    public override void OnUnequip(PlayerManager player)
    {
        player.CanUseHealthAsMana = false;
        TextOutputter.Instance.OutputText("Ouroboros Rite: The cycle ends.");
    }
}
