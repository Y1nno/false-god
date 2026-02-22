using UnityEngine;

public class XPManager : GameModule, IObserver
{
    public int level { get; private set; } = 1;
    public int currentXP { get; private set; } = 0;
    public int xpThresholdForLevelUp { get; private set; } = 10;


    #region XP Calculation Constants
    private readonly int XPThresholdBase = 10;
    private readonly float XPThresholdMultiplier = 1f;
    private readonly float XPThresholdExponent = 1f;
    private readonly int XPThresholdFlatIncrease = 20;
    #endregion

    #region XP Award Constants
    private readonly int k_XPForEncounterResolve = 1;
    private readonly int k_XPForEnemyDefeat = 1;
    private readonly int k_XPForQuestComplete = 5;
    #endregion

    private PlayerStatBox _statBox = null;

    public override void AttachDefaultObservers()
    {
        _statBox = RunManager.Instance.GetService<PlayerManager>().PlayerStats;
        AttachObserver(_statBox);
    }

    // API Methods
    #region API Methods
    public void AddXP(int amount)
    {
        currentXP += amount;
        Notify(EventType.XPAdded);
        TextOutputter.Instance.OutputText("Gained " + amount + " XP.");
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (currentXP >= xpThresholdForLevelUp)
        {
            currentXP -= xpThresholdForLevelUp;
            level++;
            TextOutputter.Instance.OutputText("Leveled up to level " + level + "!");
            xpThresholdForLevelUp = CalculateXPToNextLevel();
            Notify(EventType.LevelUp);
        }
    }

    private int CalculateXPToNextLevel()
    {
        return Mathf.FloorToInt(xpThresholdForLevelUp + XPThresholdFlatIncrease + (XPThresholdBase * Mathf.Pow(level, XPThresholdExponent) * XPThresholdMultiplier));
    }

    #endregion

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.EncounterResolve:
                AddXP(k_XPForEncounterResolve);
                break;
            case EventType.EnemyDefeated:
                AddXP(k_XPForEnemyDefeat);
                break;
            case EventType.QuestComplete:
                AddXP(k_XPForQuestComplete);
                break;
            default:
                break;
        }
    }
}