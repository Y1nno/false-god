using UnityEngine;
using System.Collections.Generic;

public class ActionFactory
{
    public ActionFactory()
    {
    }

    public CombatAction CreateActionByID(int actionID)
    {
        switch (actionID)
        {
            case 0: return new DoNothingAction();
            case 1: return new PlayerAttackAction();
            case 2: return new PlayerBlockAction();
            case 3: return new FireballAction();
            case 999: return new SwitchWeaponAction();
            case 101: return new RottenCleaverAction();
            case 102: return new DemonicSacrificeAction();
            case 103: return new BloodDrainAction();
            case 104: return new DivineShieldAction();
            case 201: return new BashAction();
            case 202: return new StealGoldAction();
            case 203: return new CurseAction();
            case 204: return new DevourAction();
            case 205: return new EscapeAction();
            case 206: return new CharmAction();
            default: return null;
        }
    }
}
