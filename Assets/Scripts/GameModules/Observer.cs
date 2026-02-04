using UnityEngine;

public interface IObserver
{
    // Notification method called when an observed subject triggers an event
    public virtual void OnNotify(object subject, EventType eventType){}
}

public enum EventType
{
    PlayerDeath,
    EncounterStart,
    EncounterResolve,
    EnemyDefeat,
    DungeonRoomAdvance,
    DungeonFloorAdvance,
    GoldAdded,
    GoldSpent,
    ItemAcquired,
    ItemRemoved,
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
}
