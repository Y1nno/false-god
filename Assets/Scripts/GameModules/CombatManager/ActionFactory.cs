using UnityEngine;
using System.Collections.Generic;

public class ActionFactory
{
    private Dictionary<int, CombatAction> availableActions = new Dictionary<int, CombatAction>();
    public ActionFactory()
    {
        InitializeActions();
    }

    public CombatAction CreateActionByID(int actionID)
    {
        if (availableActions.ContainsKey(actionID))
        {
            return availableActions[actionID];
        }
        return null;
    }

    private void InitializeActions()
    {
        // TODO: Implementation for initializing available actions
        availableActions.Add(0, new DoNothingAction());
        availableActions.Add(1, new PlayerAttackAction());
        availableActions.Add(2, new PlayerBlockAction());
        availableActions.Add(3, new FireballAction());
    }
}
