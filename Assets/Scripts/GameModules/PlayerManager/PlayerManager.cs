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
    public float CritChance => CalculateSecondaryStat(SecondaryStat.CRIT) / 100.0f;
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
        int baseStat = PlayerStats.GetStat(stat);
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        
        if (eqm == null) return baseStat;

        int bonus = stat switch
        {
            Stat.STR => eqm.GetTotalSTR(),
            Stat.DEX => eqm.GetTotalDEX(),
            Stat.INT => eqm.GetTotalINT(),
            Stat.SPD => eqm.GetTotalSPD(),
            _ => 0
        };

        return baseStat + bonus;
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
        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();

        switch (secondaryStat)
        {
            case SecondaryStat.SPATK:
                int spatkFromEquipment = eqm?.GetTotalSpecialAttack() ?? 0;
                float spatkMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.SPATK);
                return spatkFromEquipment * (int)(1.0f + spatkMultiplierFromRite);
            case SecondaryStat.SPDEF:
                int spdefFromEquipment = eqm?.GetTotalSpecialDefense() ?? 0;
                float spdefMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.SPDEF);
                return spdefFromEquipment * (int)(1.0f + spdefMultiplierFromRite);
            case SecondaryStat.PHATK:
                int phatkFromEquipment = eqm?.GetTotalPhysicalAttack() ?? 0;
                float phatkMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.PHATK);
                return phatkFromEquipment * (int)(1.0f + phatkMultiplierFromRite);
            case SecondaryStat.PHDEF:
                int phdefFromEquipment = eqm?.GetTotalPhysicalDefense() ?? 0;
                float phdefMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.PHDEF);
                return phdefFromEquipment * (int)(1.0f + phdefMultiplierFromRite);
            case SecondaryStat.CRIT:
                int critFromEquipment = eqm?.GetTotalBonus(item => item.CritChance.value) ?? 0;
                int critFromRite = (int)(RunManager.Instance.GetService<RiteManager>().CalculateFlatStatBonus(SecondaryStat.CRIT) * 100f) ;
                return critFromEquipment + critFromRite;
            case SecondaryStat.EVDE:
                return eqm?.GetTotalBonus(item => item.DodgeChance.value) ?? 0;
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


