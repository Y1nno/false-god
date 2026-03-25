using UnityEngine;

[System.Serializable]
public class Ailment
{
    public AilmentType Type;
    public int RoundsRemaining;
    public int InitialDuration;
    public int Stacks;
    
    // An optional reference back to the combatant this ailment is applied to
    // depending on if logic requires querying their specific max HP, etc. inside here
    // or if the combatant handles the logic themselves in OnRoundEnd.
    // For this design, Combatant itself handles the math, so we just track metadata here!
    
    public Ailment(AilmentType type, int duration, int stacks = 1)
    {
        Type = type;
        RoundsRemaining = duration;
        InitialDuration = duration;
        Stacks = Mathf.Clamp(stacks, 1, 5);
    }

    public void DecrementDuration()
    {
        RoundsRemaining--;
    }
}
