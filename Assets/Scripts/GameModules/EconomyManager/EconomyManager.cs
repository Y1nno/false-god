using UnityEngine;

public class EconomyManager : GameModule, IObserver
{
    private int _gold = 0;

    // Tracks the most recent change in gold (positive or negative)
    public int GoldDelta { get; private set; } = 0;
    public int GoldSpentInRun { get; set; } = 0;

    public override void AttachDefaultObservers()
    {
        // none for now
    }

    public int AddGold(int amount)
    {
        _gold += amount;
        GoldDelta = amount; // Keep GoldDelta assignment
        TextOutputter.Instance.OutputText($"Gained {amount} gold. Total: {_gold}"); // Updated text
        Notify(EventType.GoldAdded);
        RunManager.Instance.GetService<SaveManager>()?.SaveRun(); // Added SaveRun call
        return _gold;
    }

    public bool SpendGold(int amount) // Changed return type to bool
    {
        if (CanSpendGold(amount))
        {
            _gold -= amount;
            GoldDelta = -amount;
            GoldSpentInRun += amount;
            TextOutputter.Instance.OutputText($"Spent {amount} gold. Remaining: {_gold}"); // Updated text
            Notify(EventType.GoldSpent);
            RunManager.Instance.GetService<SaveManager>()?.SaveRun(); // Added SaveRun call
            return true; // Added return true
        }
        return false; // Added return false
    }

    public void LoseGold(int amount)
    {
        _gold = Mathf.Max(0, _gold - amount);
        GoldDelta = -amount;
        Notify(EventType.GoldSpent);
        RunManager.Instance.GetService<SaveManager>()?.SaveRun(); // Added SaveRun call
    }

    public bool CanSpendGold(int amount)
    {
        return _gold >= amount;
    }

    public int GetCurrentGold()
    {
        return _gold;
    }

    public void RestoreState(int gold)
    {
        _gold = gold;
        Notify(EventType.GoldAdded); // Trigger UI refresh
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
