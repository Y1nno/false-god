using UnityEngine;

public interface IObserver
{
    // Notification method called when an observed subject triggers an event
    public void OnNotify(object subject, EventType eventType){}
    //public virtual void OnNotify(Subject subject, EventType eventType){}
}

public enum EventType
{
    PlayerDeath,
    EncounterStart,
    EncounterResolve,
    PlayerAboutToDie,
    DungeonRoomAdvance,
    DungeonFloorAdvance,
    GoldAdded,
    GoldSpent,
    ItemAcquired,
    ItemRemoved,
    EquipmentChanged,
    LevelUp,
    QuestComplete,
    XPAdded,
    StatPointsAdded,
    RoundStart,
    RoundEnd,
    PlayerTurnStart,
    PlayerTurnEnd,
    EnemyTurnStart,
    EnemyTurnEnd,
    BattleStart,
    BattleEnd,
    PlayerActionSet,
    EnemyDefeated,
    DiceRoll,
    DecisionMade
}
