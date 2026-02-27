using System;
using UnityEngine;


public abstract class QuestDataSO : ScriptableObject
{
    public abstract bool CanComplete();
    public abstract bool TryComplete();
    public abstract string GetCurrentRequirementDescription();
}