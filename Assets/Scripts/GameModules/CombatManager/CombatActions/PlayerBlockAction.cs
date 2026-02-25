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

    public override void Execute(Combatant user , Combatant target = null)
    {
        TextOutputter.Instance.OutputText($"{user.GetName()} is blocking and will take reduced damage next turn!");
    }
}
