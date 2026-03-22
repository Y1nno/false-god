using UnityEngine;

public class XPManager : GameModule, IObserver
{
    public int level { get; private set; } = 1;
    public int currentXP { get; private set; } = 0;
    public int xpThresholdForLevelUp { get; private set; } = 13;

    #region XP Award Constants
    private readonly int k_XPForQuestComplete = 5;
    #endregion

    private PlayerStatBox _statBox = null;

    public override void AttachDefaultObservers()
    {
        _statBox = RunManager.Instance.GetService<PlayerManager>().PlayerStats;
        AttachObserver(_statBox);
        
        RunManager.Instance.GetService<EncounterManager>()?.AttachObserver(this);
        RunManager.Instance.GetService<CombatManager>()?.AttachObserver(this);
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
        // Formula: XPn = XPn-1 + 10 + (Level^2 * 3)
        return xpThresholdForLevelUp + 10 + (level * level * 3);
    }

    #endregion

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.EnemyDefeated:
                if (subject is Enemy enemy)
                {
                    int xpAward = Mathf.RoundToInt(enemy.BaseXP + enemy.Level * 0.25f);
                    AddXP(xpAward);
                }
                break;
            case EventType.QuestComplete:
                AddXP(k_XPForQuestComplete);
                break;
            default:
                break;
        }
    }
}