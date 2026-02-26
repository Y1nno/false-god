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

    public float HealingMultiplier = 1.0f;
    public float ManaRecoveryPercentFromReligion { get; private set; } = 0.0f;
    public int freeRepairs = 0;

    public readonly Dictionary<Stat, float> StartOfCombatStatBonuses = new Dictionary<Stat, float>();
    public readonly Dictionary<SecondaryStat, float> StartOfCombatSecondaryStatBonuses = new Dictionary<SecondaryStat, float>();
    private Dictionary<Passive, PlayerPassive> _passives = new Dictionary<Passive, PlayerPassive>();

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
        int finalHeal = Mathf.RoundToInt(amount * HealingMultiplier);
        Health.Increase(finalHeal);
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
        int religionBonus = RunManager.Instance.GetService<ReligionManager>().GetReligionBonus(secondaryStat);
        float religionMultiplier = RunManager.Instance.GetService<ReligionManager>().GetReligionMultiplier(secondaryStat);
        switch (secondaryStat)
        {
            case SecondaryStat.SPATK:
                int spatkFromEquipment = RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.SPATK);
                float spatkMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.SPATK);
                return spatkFromEquipment + religionBonus * Mathf.RoundToInt(religionMultiplier * spatkMultiplierFromRite);
            case SecondaryStat.SPDEF:
                int spdefFromEquipment = RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.SPDEF);
                float spdefMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.SPDEF);
                return spdefFromEquipment + religionBonus * Mathf.RoundToInt(religionMultiplier * spdefMultiplierFromRite);
            case SecondaryStat.PHATK:
                int phatkFromEquipment = RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.PHATK);
                float phatkMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.PHATK);
                return phatkFromEquipment + religionBonus * Mathf.RoundToInt(religionMultiplier * phatkMultiplierFromRite);
            case SecondaryStat.PHDEF:
                int phdefFromEquipment = RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.PHDEF);
                float phdefMultiplierFromRite = RunManager.Instance.GetService<RiteManager>().CalculateStatMultiplierFromRites(SecondaryStat.PHDEF);
                return phdefFromEquipment + religionBonus * Mathf.RoundToInt(religionMultiplier * phdefMultiplierFromRite);
            case SecondaryStat.CRIT:
                int critFromEquipment = RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.CRIT);
                int critFromRite = (int)(RunManager.Instance.GetService<RiteManager>().CalculateFlatStatBonus(SecondaryStat.CRIT) * 100f) ;
                return critFromEquipment + critFromRite + religionBonus * Mathf.RoundToInt(religionMultiplier);
            case SecondaryStat.EVDE:
                return RunManager.Instance.GetService<InventoryManager>().CalculateSecondaryStatFromEquipment(SecondaryStat.EVDE) + religionBonus * Mathf.RoundToInt(religionMultiplier);
            default:
                throw new ArgumentOutOfRangeException(nameof(secondaryStat), secondaryStat, null);
        }

    }

    public void AddManaRecoveryPercent(float amount)
    {
        ManaRecoveryPercentFromReligion += amount;
    }

    public void RemoveManaRecoveryPercent(float amount)
    {
        ManaRecoveryPercentFromReligion = Mathf.Max(0f, ManaRecoveryPercentFromReligion - amount);
    }

    public void AddStartOfCombatStatBuff(Stat stat, float amount)
    {
        if (!StartOfCombatStatBonuses.ContainsKey(stat))
        {
            StartOfCombatStatBonuses[stat] = 0f;
        }

        StartOfCombatStatBonuses[stat] += amount;
    }

    public void RemoveStartOfCombatStatBuff(Stat stat, float amount)
    {
        if (!StartOfCombatStatBonuses.ContainsKey(stat))
        {
            return;
        }

        StartOfCombatStatBonuses[stat] -= amount;
        if (Mathf.Approximately(StartOfCombatStatBonuses[stat], 0f))
        {
            StartOfCombatStatBonuses.Remove(stat);
        }
    }

    public void AddStartOfCombatSecondaryStatBuff(SecondaryStat stat, float amount)
    {
        if (!StartOfCombatSecondaryStatBonuses.ContainsKey(stat))
        {
            StartOfCombatSecondaryStatBonuses[stat] = 0f;
        }

        StartOfCombatSecondaryStatBonuses[stat] += amount;
    }

    public void RemoveStartOfCombatSecondaryStatBuff(SecondaryStat stat, float amount)
    {
        if (!StartOfCombatSecondaryStatBonuses.ContainsKey(stat))
        {
            return;
        }

        StartOfCombatSecondaryStatBonuses[stat] -= amount;
        if (Mathf.Approximately(StartOfCombatSecondaryStatBonuses[stat], 0f))
        {
            StartOfCombatSecondaryStatBonuses.Remove(stat);
        }
    }

    public void AddPassive(Passive passive)
    {
        _passives.Add(passive, new PlayerPassive());
    }

    public void RemovePassive(Passive passive)
    {
        _passives.Remove(passive);
    }

    public bool HasPassive(Passive passive)
    {
        return _passives.ContainsKey(passive);
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


