using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Battle : Subject, IObserver
{
    public List<Combatant> combatants = new List<Combatant>();
    PlayerManager _pm = RunManager.Instance.GetService<PlayerManager>();
    public PlayerCombatManager Pcm { get; private set; }
    System.Random rng = new System.Random();
    private bool _turnRefreshedThisEncounter = false;
    private bool _needsTurnRefresh = false;
    public int TurnCount { get; private set; } = 1;

    public Battle(List<Combatant> enemies, PlayerCombatManager player)
    {
        combatants = enemies;
        Pcm = player;
        combatants.Add(Pcm);
        foreach (Combatant combatant in combatants)
        {
            combatant.AttachObserver(this);
        }
    }

    public void StartBattle()
    {
        TextOutputter.Instance.OutputText("A battle has started!");
        foreach (Enemy enemy in GetEnemies())
        {
            TextOutputter.Instance.OutputText($"Enemy encountered: {enemy.Name} (HP: {enemy.Health.CurrentValue})");
        }

        StartRound();
    }

    public bool CheckBattleOngoing()
    {
        return _pm.Health.CurrentValue > 0 && HasLivingEnemies();
    }

    private bool HasLivingEnemies()
    {
        return GetEnemies().Count > 0;
    }

    public void StartRound()
    {
        //Debug.Log($"StartRound called, frame={Time.frameCount}");
        if (!CheckBattleOngoing())
        {
            Notify(EventType.BattleEnd);
            return;
        }
        Notify(EventType.RoundStart);
        if (TurnCount > 1) 
        {
        }
        SetActions();
    }

    public void EndRound()
    {
        // Process over-time effects before actually ending
        foreach (Combatant combatant in combatants)
        {
            if (combatant.IsAlive())
            {
                combatant.OnRoundEnd();
            }
        }

        RunManager.Instance.GetService<EquipmentManager>()?.TickCooldowns(CooldownType.CombatRounds);

        //Debug.Log($"EndRound called, frame={Time.frameCount}");

        InputInterface inputUI = GameObject.FindAnyObjectByType<InputInterface>();
        if (inputUI != null) inputUI.SendMessage("RefreshUI");

        if (!CheckBattleOngoing())
        {
            Notify(EventType.BattleEnd);
            return;
        }
        Notify(EventType.RoundEnd);
        TurnCount++;
        StartRound();
    }

    // Detach this battle as an observer from all combatants to prevent ghost battles
    public void Cleanup()
    {
        foreach (Combatant combatant in combatants)
        {
            combatant.DetachObserver(this);
        }
    }
    public void SetActions()
    {
        //Debug.Log("Setting actions for all combatants.");
        foreach (Enemy enemy in GetEnemies())
        {
            enemy.ChooseAction();
        }
        TelegraphEnemyActions();
        Pcm.ChooseAction();
    }

    private void TelegraphEnemyActions()
    {
        foreach (Enemy enemy in GetEnemies())
        {
            TextOutputter.Instance.OutputText($"Enemy {enemy.Name} is preparing to use {enemy.CurrentAction.ActionName}!");
        }
    }

    public List<Combatant> GetEnemies(bool aliveOnly = true)
    {
        List<Combatant> enemies = new List<Combatant>();
        foreach (Combatant combatant in combatants)
        {
            if (combatant is Enemy && (!aliveOnly || combatant.IsAlive()))
            {
                enemies.Add(combatant);
            }
        }
        return enemies;
    }

    public void CalculateResolutionOrder()
    {
        // Sort combatants by Action Priority first, then SPD stat in descending order
        combatants = combatants
            .OrderByDescending(c => c.CurrentAction != null ? c.CurrentAction.Priority : 0)
            .ThenByDescending(c => c.GetStat(Stat.SPD))
            .ThenBy(_ => rng.Next()).ToList();
    }

    public void ResolveCombatantsActions()
    {
        foreach (Combatant combatant in combatants)
        {
            if (combatant.IsAlive())
            {
                combatant.ExecuteAction();
            }
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        //Debug.Log($"[Battle.OnNotify] got {eventType} ({(int)eventType}) from battle object this={this.GetHashCode()}, subject={(Combatant)subject}, frame={Time.frameCount}");
        switch (eventType)
        {
            case EventType.PlayerActionSet:
                //Debug.Log($"Player action set, calculating resolution order and resolving actions. Called from frame={Time.frameCount} by {((Combatant)subject).GetName()}");
                CalculateResolutionOrder();
                ResolveCombatantsActions();
                
                if (_needsTurnRefresh)
                {
                    _needsTurnRefresh = false;
                    SetActions();
                }
                else
                {
                    EndRound();
                }
                break;
            case EventType.EnemyDefeated:
                // Notify observers (CombatManager) that an enemy was defeated, passing the enemy object
                Notify(subject, EventType.EnemyDefeated);
                
                RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
                if (relicm != null && relicm.HasTurnRefreshOnKill() && !_turnRefreshedThisEncounter)
                {
                    _turnRefreshedThisEncounter = true;
                    _needsTurnRefresh = true;
                    TextOutputter.Instance.OutputText("Relic of Eternal Hunt glows! Your turn is refreshed.");
                }
                break;
            default:
                break;
        }
    }

}
