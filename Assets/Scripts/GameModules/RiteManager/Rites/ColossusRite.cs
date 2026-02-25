using UnityEngine;

public class ColossusRite : Rite, IObserver
{
    private const int k_DamageBonus = 3;
    private bool _isBonusActive = false;
    private PlayerManager _player;

    public override string Description => $"+{k_DamageBonus} DMG (2H Weapon)";

    public ColossusRite() : base("Colossus", 2, RiteType.Colossus) // ID: Colossus, Cost: 2
    {
        RiteType = RiteType.Colossus;
    }

    public override void OnEquip(PlayerManager player)
    {
        _player = player;
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null)
        {
            eqm.AttachObserver(this);
            CheckAndUpdateBonus(eqm);
        }
    }

    public override void OnUnequip(PlayerManager player)
    {
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null)
        {
            eqm.DetachObserver(this);
        }
        if (_isBonusActive)
        {
            player.BonusDamage -= k_DamageBonus;
            _isBonusActive = false;
        }
        _player = null;
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EquipmentChanged && subject is EquipmentManager eqm)
        {
            CheckAndUpdateBonus(eqm);
        }
    }

    private void CheckAndUpdateBonus(EquipmentManager eqm)
    {
        if (_player == null) return;
        
        bool holdsTwoHanded = eqm.IsHoldingTwoHandedWeapon();
        
        if (holdsTwoHanded && !_isBonusActive)
        {
            _player.BonusDamage += k_DamageBonus;
            _isBonusActive = true;
        }
        else if (!holdsTwoHanded && _isBonusActive)
        {
            _player.BonusDamage -= k_DamageBonus;
            _isBonusActive = false;
        }
    }
}
