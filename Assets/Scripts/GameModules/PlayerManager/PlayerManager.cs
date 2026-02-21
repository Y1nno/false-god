using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerManager : GameModule
{
    private const int k_StartingMaxHealth = 100;
    private const int k_StartingMaxMana = 50;

    private const float k_HealthPerStrength = 2.0f;
    private const float k_ManaPerIntelligence = 1.5f;

    public Resource Health = new Resource(k_StartingMaxHealth);
    public Resource Mana = new Resource(k_StartingMaxMana);

    public int BonusDamage { get; set; } = 0;
    public float DamageMultiplier { get; set; } = 1.0f;
    public float CritChance { get; set; } = 0.0f;
    public bool CanUseHealthAsMana { get; set; } = false;
    public int DiceRerollCount { get; set; } = 0;

    public PlayerStatBox PlayerStats { get; private set; } = new PlayerStatBox();

    public override void AttachDefaultObservers()
    {
        // none for now
    }

    //API methods

    #region Health and Mana Management
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        Health.Decrease(amount);
        TextOutputter.Instance.OutputText($"Took {amount} damage.");
        if (Health.CurrentValue <= 0)
        {
            // Give observers (like Lazarus Rite) a chance to save the player
            Notify(EventType.PlayerAboutToDie);

            // Re-check health after observers have run
            if (Health.CurrentValue <= 0)
            {
                Die();
            }
        }
    }

    public bool TryUseMana(int amount)
    {
        if (!Mana.CanAfford(amount))
        {
            return false;
        }
        Mana.Decrease(amount);
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        Health.Increase(amount);
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0) return;
        Mana.Increase(amount);
    }
    #endregion

    #region Stat Management

    public int GetStat(Stat stat)
    {
        return PlayerStats.GetStat(stat);
    }

    public void SetStat(Stat stat, int value)
    {
        PlayerStats.SetStat(stat, value);
        if (stat == Stat.STR)
        {
            int mod = Mathf.RoundToInt((GetStat(Stat.STR) * k_HealthPerStrength));
            Health.SetStatModifier(mod);
        }
        else if (stat == Stat.INT)
        {
            int mod = Mathf.RoundToInt((GetStat(Stat.INT) * k_ManaPerIntelligence));
            Mana.SetStatModifier(mod);
        }
    }

    public void IncreaseStat(Stat stat, int amount)
    {
        int currentValue = PlayerStats.GetStat(stat);
        SetStat(stat, currentValue + amount);
    }

    public void DecreaseStat(Stat stat, int amount)
    {
        int currentValue = PlayerStats.GetStat(stat);
        SetStat(stat, currentValue - amount);
    }

    public int CalculateSecondaryStat(SecondaryStat secondaryStat)
    {
        switch (secondaryStat)
        {
            //TODO: Implement formulas
            case SecondaryStat.SPATK:
                return 1;
            case SecondaryStat.SPDEF:
                return 1;
            case SecondaryStat.CRIT:
                return 1;
            case SecondaryStat.EVDE:
                return 1;
            default:
                throw new ArgumentOutOfRangeException(nameof(secondaryStat), secondaryStat, null);
        }

    }
    #endregion

    #region Dice Rolling
    public void RollForStat(Stat stat, int successThreshold)
    {
        DiceRoller.Instance.RollForStat(stat, successThreshold);
    }

    public void Reroll()
    {
        DiceRoller.Instance.Reroll();
    }
    #endregion

    private void Die()
    {
        // Notify observers about player death
        Notify(EventType.PlayerDeath);
    }
}


