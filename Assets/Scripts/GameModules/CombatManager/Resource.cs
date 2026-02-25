using UnityEngine;

public class Resource
{
    public int CurrentValue { get; private set; }
    public int MaxValue { get; private set; }
    public int StatModifier { get; private set; }
    public float PercentageModifier { get; private set; }

    private int _baseMax;

    public Resource(int baseMax)
    {
        _baseMax = baseMax;
        RecalculateMax();
        CurrentValue = MaxValue;
    }

    public void Decrease(int amount)
    {
        if (amount <= 0) return;
        CurrentValue = Mathf.Max(0, CurrentValue - amount);
    }

    public void Increase(int amount)
    {
        if (amount <= 0) return;
        CurrentValue = Mathf.Min(MaxValue, CurrentValue + amount);
    }

    public bool CanAfford(int amount) => CurrentValue >= amount;

    public void RestoreToFull() => CurrentValue = MaxValue;

    // Sets the BASE max (not including stat modifier)
    public void SetBaseMax(int newBaseMax)
    {
        _baseMax = Mathf.Max(0, newBaseMax);
        RecalculateMax();
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);
    }

    public void IncreaseBaseMax(int amount)
    {
        SetBaseMax(_baseMax + amount);
    }

    public void SetCurrent(int newCurrent)
    {
        CurrentValue = Mathf.Clamp(newCurrent, 0, MaxValue);
    }

    public void SetStatModifier(int modifier)
    {
        StatModifier = modifier;
        RecalculateMax();
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);
    }

    public void SetPercentageModifier(float modifier)
    {
        PercentageModifier = modifier;
        RecalculateMax();
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);
    }

    private void RecalculateMax()
    {
        int rawMax = Mathf.Max(0, _baseMax + StatModifier);
        MaxValue = Mathf.FloorToInt(rawMax * (1f + PercentageModifier));
    }
}

