using UnityEngine;

public class EnemyEncounter : Encounter, IObserver
{
    private CombatManager _combatManager;
    public EnemyEncounter(float difficulty) : base(difficulty)
    {
        _combatManager = RunManager.Instance.GetService<CombatManager>();
        _combatManager.AttachObserver(this);
    }

    public override void StartEncounter()
    {
        base.StartEncounter();
        _combatManager.CreateNewBattle(_difficulty);
        _combatManager.Start();
    }

    public override void RecieveDecision(int decisionIndex)
    {
        _combatManager.CurrentBattle.Pcm.RecieveDecision(decisionIndex);
    }

    public override void ResolveEncounter()
    {
        _combatManager.InstaKillCurrentBattle();
        _combatManager.DetachObserver(this);
        base.ResolveEncounter();
    }

    public void OnNotify(object subject, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.BattleEnd:
                Debug.Log("EnemyEncounter received BattleEnd event, resolving encounter");
                ResolveEncounter();
                break;
            default:
                break;
        }
    }
}
