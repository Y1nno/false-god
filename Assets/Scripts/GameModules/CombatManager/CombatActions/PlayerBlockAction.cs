using UnityEngine;

public class PlayerBlockAction : CombatAction
{
    public PlayerBlockAction()
    {
        ActionID = 3;
        ActionName = "Block";
        ManaCost = 0;
        TargetType = TargetingType.Self;
    }

    public override bool CanUse(Combatant user)
    {
        if (!base.CanUse(user)) return false;

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm == null) return false;

        Equipment weapon = eqm.GetEquippedItem(EquipmentSlot.Weapon);
        Equipment offhand = eqm.GetEquippedItem(EquipmentSlot.OffHand);

        bool hasTwoHanded = weapon != null && weapon.IsTwoHanded;
        bool hasShield = offhand != null; // Offhand is treated as shield

        return hasTwoHanded || hasShield;
    }

    public override void Execute(Combatant user, Combatant target = null)
    {
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        int totalBlock = 0;

        if (eqm != null)
        {
            Equipment weapon = eqm.GetEquippedItem(EquipmentSlot.Weapon);
            Equipment offhand = eqm.GetEquippedItem(EquipmentSlot.OffHand);

            int baseBlock = 0;
            // Only 2-handed weapons or shields (offhand) provide base block
            if (weapon != null && weapon.IsTwoHanded) 
                baseBlock = Mathf.Max(baseBlock, eqm.GetBaseBlockAmount(weapon.Rarity));
            
            if (offhand != null) 
                baseBlock = Mathf.Max(baseBlock, eqm.GetBaseBlockAmount(offhand.Rarity));

            // Bonus Block from traits also only applies if you have a valid blocking setup
            int bonusBlock = eqm.GetTotalBonusBlock();
            float multiplier = eqm.GetTotalShieldingMultiplier();
            totalBlock = Mathf.RoundToInt((baseBlock + bonusBlock) * multiplier);
        }

        user.CurrentBlock += totalBlock;
        TextOutputter.Instance.OutputText($"{user.GetName()} defends! Gained {totalBlock} Shield (Total: {user.CurrentBlock}).");
    }
}
