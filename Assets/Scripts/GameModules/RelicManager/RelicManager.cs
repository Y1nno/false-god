using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RelicManager : GameModule, IObserver
{
    private List<RelicInstance> _activeRelics = new List<RelicInstance>();
    private int _enemiesKilledThisRun = 0;

    public override void AttachDefaultObservers()
    {
        RunManager.Instance.GetService<CombatManager>()?.AttachObserver(this);
        RefreshRelics();
    }

    public bool HasRelic(string relicName)
    {
        RefreshRelics();
        return _activeRelics.Any(r => r.GetName() == relicName || r.BaseData.ItemName == relicName);
    }

    public void RefreshRelics()
    {
        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        if (invm == null) return;

        _activeRelics = invm.UnEquippedItems.OfType<RelicInstance>().ToList();
    }

    public float GetGoldMultiplier()
    {
        RefreshRelics();
        float bonus = 0;
        foreach (var relic in _activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.GoldBonus))
        {
            bonus += relic.BaseData.EffectValue;
        }
        return 1.0f + bonus;
    }

    public float GetDamageMultiplier(Combatant target = null)
    {
        RefreshRelics();
        float multiplier = 1.0f;

        foreach (var relic in _activeRelics)
        {
            switch (relic.BaseData.EffectType)
            {
                case RelicEffectType.DamageBonus:
                    if (relic.BaseData.ItemName == "Hoarder's Fetish")
                    {
                        multiplier += (_activeRelics.Count * relic.BaseData.EffectValue);
                    }
                    else
                    {
                        multiplier += relic.BaseData.EffectValue;
                    }
                    break;
                case RelicEffectType.DamageVsBosses:
                    if (target != null && target.IsBoss)
                    {
                        multiplier += relic.BaseData.EffectValue;
                    }
                    break;
                case RelicEffectType.UniqueEnemyChance:
                    // Whispering Reliquary gives -5% damage as a drawback for higher unique chance
                    if (relic.BaseData.ItemName == "Whispering Reliquary")
                    {
                        // TODO: Implement Unique Enemy encounter chance bonus
                        multiplier -= 0.05f;
                    }
                    break;
            }
        }

        // Add damage from Thousand Duels (turn based)
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        if (cm != null && cm.CurrentBattle != null)
        {
            float duelBonus = GetDuelDamageBonus(cm.CurrentBattle.TurnCount);
        }

        // Add damage from Hunter's Trophy
        multiplier += GetHunterTrophyMultiplier();

        return multiplier;
    }

    public int GetFlatDamageBonus()
    {
        RefreshRelics();
        int bonus = 0;
        
        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        if (cm != null && cm.CurrentBattle != null)
        {
            bonus += (int)GetDuelDamageBonus(cm.CurrentBattle.TurnCount);
        }

        return bonus;
    }

    private float GetDuelDamageBonus(int turnCount)
    {
        var relic = _activeRelics.FirstOrDefault(r => r.BaseData.EffectType == RelicEffectType.DamagePerTurnSpent);
        if (relic != null)
        {
            return turnCount * relic.BaseData.EffectValue;
        }
        return 0;
    }

    private float GetHunterTrophyMultiplier()
    {
   
        return 0;
    }

    public int GetHunterTrophyFlatBonus()
    {
        var relic = _activeRelics.FirstOrDefault(r => r.BaseData.EffectType == RelicEffectType.DamagePerKillCount);
        if (relic != null)
        {
            return (_enemiesKilledThisRun / 10) * (int)relic.BaseData.EffectValue;
        }
        return 0;
    }

    public int GetMaxHPBonus()
    {
        RefreshRelics();
        return (int)_activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.MaxHpBonus).Sum(r => r.BaseData.EffectValue);
    }

    public int GetFlatDamageReduction()
    {
        RefreshRelics();
        return (int)_activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.FlatDamageReduction).Sum(r => r.BaseData.EffectValue);
    }

    public float GetShopPriceMultiplier()
    {
        RefreshRelics();
        float reduction = 0;
        foreach (var relic in _activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.ShopPriceReduction))
        {
            reduction += relic.BaseData.EffectValue;
        }
        return 1.0f - reduction;
    }

    public float GetCritDamageMultiplier()
    {
        RefreshRelics();
        float bonus = 0;
        foreach (var relic in _activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.CritDamageBonus))
        {
            bonus += relic.BaseData.EffectValue;
        }
        return 2.0f + bonus;
    }

    public float GetEnemyDamageMultiplier()
    {
        RefreshRelics();
        float multi = 1.0f;
        if (_activeRelics.Any(r => r.BaseData.ItemName == "Crown of the Damned"))
        {
            multi += 0.1f;
        }
        return multi;
    }

    public float GetSplashDamagePercentage()
    {
        RefreshRelics();
        var bestRelic = _activeRelics
            .Where(r => r.BaseData.EffectType == RelicEffectType.SplashDamage)
            .OrderByDescending(r => r.BaseData.EffectValue)
            .FirstOrDefault();
        
        return bestRelic?.BaseData.EffectValue ?? 0f;
    }

    public bool HasTurnRefreshOnKill()
    {
        RefreshRelics();
        return _activeRelics.Any(r => r.BaseData.EffectType == RelicEffectType.TurnRefreshOnKill);
    }

    public bool TryFreeMana()
    {
        RefreshRelics();
        var relic = _activeRelics.FirstOrDefault(r => r.BaseData.EffectType == RelicEffectType.FreeManaChance);
        if (relic != null)
        {
            return Random.value <= relic.BaseData.EffectValue;
        }
        return false;
    }

    public int GetHpLossPerTurn()
    {
        RefreshRelics();
        int loss = (int)_activeRelics.Where(r => r.BaseData.EffectType == RelicEffectType.HpLossPerTurn).Sum(r => r.BaseData.EffectValue);
        
        // Handle dual-effect relics by name
        if (_activeRelics.Any(r => r.BaseData.ItemName == "Idol of Endless Hunger"))
        {
            loss += 1;
        }

        return loss;
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EnemyDefeated)
        {
            _enemiesKilledThisRun++;
        }
    }
}
