using UnityEngine;

public class EconomyManager : Subject
{
    private int _gold = 0;
    public int GoldDelta { get; private set; } = 0;

    public int AddGold(int amount)
    {
        _gold += amount;
        GoldDelta = amount;
        TextOutputter.Instance.OutputText($"Gained {amount} gold.");
        Notify(EventType.GoldAdded);
        return _gold;
    }

    public int SpendGold(int amount)
    {
        if (CanSpendGold(amount))
        {
            _gold -= amount;
            GoldDelta = -amount;
            TextOutputter.Instance.OutputText($"Spent {amount} gold.");
            Notify(EventType.GoldSpent);
        }
        return _gold;
    }

    public bool CanSpendGold(int amount)
    {
        return _gold >= amount;
    }

    public int GetCurrentGold()
    {
        return _gold;
    }

}
