using UnityEngine;

public class EnemyEncounter : Encounter, IObserver
{
    private CombatManager _combatManager;
    public EnemyEncounter(float difficulty) : base(difficulty)
    {
        _combatManager = RunManager.Instance.GetService<CombatManager>();
        _combatManager.CreateNewBattle(difficulty);
        _combatManager.AttachObserver(this);
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        _combatManager.Start();
    }

    public override void RecieveDecision(int decisionIndex)
    {
        _combatManager.CurrentBattle.Pcm.RecieveDecision(decisionIndex);
    }

    public void OnNotify(Subject subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.BattleEnd:
                ResolveEncounter();
                break;
            default:
                break;
        }
    }
}
