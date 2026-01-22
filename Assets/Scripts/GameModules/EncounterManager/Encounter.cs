using UnityEngine;

public class Encounter
{
    public virtual void ProcessPlayerCommand(PlayerCommand command)
    {
        Debug.Log("Base Encounter ProcessPlayerCommand called.");
    }
}
