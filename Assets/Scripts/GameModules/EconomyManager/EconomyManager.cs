using UnityEngine;

public class EconomyManager : GameModule, IObserver
{
    private int _gold = 0;

    // Tracks the most recent change in gold (positive or negative)
    public int GoldDelta { get; private set; } = 0;

    public override void AttachDefaultObservers()
    {
        // none for now
    }

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

    public void LoseGold(int amount)
    {
        _gold = Mathf.Max(0, _gold - amount);
        GoldDelta = -amount;
        Notify(EventType.GoldSpent);
    }

    public bool CanSpendGold(int amount)
    {
        return _gold >= amount;
    }

    public int GetCurrentGold()
    {
        return _gold;
    }

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.EnemyDefeated:
                if (subject is Enemy enemy)
                {
                    int gold = (int) enemy.GoldValue;
                    RiteManager rm = RunManager.Instance.GetService<RiteManager>();
                    if (rm.HasRite(RiteType.Midas))
                    {
                        MidasRite midasRite = (MidasRite)rm.GetRite(RiteType.Midas);
                        gold = midasRite.ApplyMidasEffect(gold);
                    }

                    RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
                    if (relicm != null)
                    {
                        gold = Mathf.RoundToInt(gold * relicm.GetGoldMultiplier());
                    }

                    AddGold(gold);
                }
                break;
        }
    }

}
