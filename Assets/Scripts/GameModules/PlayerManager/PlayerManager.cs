using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerManager : GameModule, IObserver
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
        RunManager.Instance.GetService<InventoryManager>()?.AttachObserver(this);
        RefreshEquipmentStats();
        Health.RestoreToFull();
        Mana.RestoreToFull();
    }

    public void RestoreState(int health, int mana)
    {
        Health.SetCurrent(health);
        Mana.SetCurrent(mana);
    }

    //API methods

    #region Health and Mana Management
    public int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;
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
        return amount;
    }

    public bool TryUseMana(int amount)
    {
        if (!Mana.CanAfford(amount))
        {
            return false;
        }
        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        if (relicm != null && relicm.TryFreeMana())
        {
            TextOutputter.Instance.OutputText("Blessed Cross glows! The spell costs no mana.");
            return true;
        }

        Mana.Decrease(amount);
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        
        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (rm?.CurrentReligion is OrderOfTheDawnbearers && rm.CurrentReligion.CurrentFaithLevel >= 1)
        {
            amount = Mathf.RoundToInt(amount * 1.10f); // 10% Heal Amp
        }

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
            Stat.SPD => eqm.GetTotalSPD() + Mathf.RoundToInt(GetStat(Stat.DEX) * 0.3f),
            // Stat.LCK => eqm.GetTotalLCK(),
            _ => 0
        };

        CombatManager cm = RunManager.Instance.GetService<CombatManager>();
        int combatBonus = cm?.Pcm?.GetStatBonus(stat) ?? 0;

        return baseStat + bonus + combatBonus;
    }

    public void SetStat(Stat stat, int value)
    {
        PlayerStats.SetStat(stat, value);
        if (stat == Stat.STR || stat == Stat.INT)
        {
            RefreshEquipmentStats();
        }
    }

    public void RefreshEquipmentStats()
    {
        // Update Health modifiers
        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        int relicHPBonus = relicm != null ? relicm.GetMaxHPBonus() : 0;

        int strMod = Mathf.RoundToInt((GetStat(Stat.STR) * k_HealthPerStrength)) + relicHPBonus;
        // Debug.Log($"Refreshing Equipment Stats: Player STR={PlayerStats.STR}, Total STR={GetStat(Stat.STR)}, Modifier={strMod}");
        Health.SetStatModifier(strMod);

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null)
        {
            float hpMultiplier = eqm.GetTotalMaxHPPercentage() / 100f;
            Health.SetPercentageModifier(hpMultiplier);
        }

        // Update Mana modifiers
        int intMod = Mathf.RoundToInt((GetStat(Stat.INT) * k_ManaPerIntelligence));
        Mana.SetStatModifier(intMod);
    }

    public bool UseStatPoints(Stat stat, int amount)
    {
        bool success = PlayerStats.SpendStatPoints(stat, amount);
        if (success)
        {
            if (stat == Stat.STR || stat == Stat.INT)
            {
                RefreshEquipmentStats();
            }
            Notify(EventType.EquipmentChanged); // Hack to trigger UI/Stat refreshes
        }
        return success;
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
                
                ReligionManager rmSp = RunManager.Instance.GetService<ReligionManager>();
                if (rmSp?.CurrentReligion is ChildrenOfThePaleMoon && Health.Percentage >= 0.8f && rmSp.CurrentReligion.CurrentFaithLevel >= 3)
                {
                    spatkMultiplierFromRite += 0.2f;
                }

                return (int)(spatkFromEquipment * (1.0f + spatkMultiplierFromRite));
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
                int critFromEquipment = eqm?.GetTotalBonus(item => item.CritChance) ?? 0;
                int critFromRite = (int)(RunManager.Instance.GetService<RiteManager>().CalculateFlatStatBonus(SecondaryStat.CRIT) * 100f) ;
                int critFromDex = Mathf.RoundToInt(GetStat(Stat.DEX) * 0.25f);
                
                int critFromReligion = 0;
                ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
                CombatManager cm = RunManager.Instance.GetService<CombatManager>();
                if (rm?.CurrentReligion is VeilOfUmbrath && cm?.CurrentBattle?.TurnCount == 1 && rm.CurrentReligion.CurrentFaithLevel >= 3)
                {
                    critFromReligion = 40;
                }
                return critFromEquipment + critFromRite + critFromDex + critFromReligion;
            case SecondaryStat.EVDE:
                int dodgeFromEquipment = eqm?.GetTotalBonus(item => item.DodgeChance) ?? 0;
                int dodgeFromCombat = RunManager.Instance.GetService<CombatManager>()?.Pcm?.GetSecondaryStatBonus(SecondaryStat.EVDE) ?? 0;
                int dodgeFromDex = Mathf.RoundToInt(GetStat(Stat.DEX) * 0.20f);
                
                int dodgeFromReligion = 0;
                ReligionManager rmDodge = RunManager.Instance.GetService<ReligionManager>();
                CombatManager cmDodge = RunManager.Instance.GetService<CombatManager>();
                if (rmDodge?.CurrentReligion is VeilOfUmbrath && cmDodge?.CurrentBattle?.TurnCount == 1 && rmDodge.CurrentReligion.CurrentFaithLevel >= 1)
                {
                    dodgeFromReligion = 30;
                }

                int totalDodge = dodgeFromEquipment + dodgeFromCombat + dodgeFromDex + dodgeFromReligion;
                return Mathf.Min(65, totalDodge); // 65% Dodge Cap
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

    public void Die()
    {
        // Notify observers about player death
        TextOutputter.Instance.OutputText("<color=red>YOU DIED.</color>");
        RunManager.Instance.GetService<SaveManager>()?.ClearRunSave();
        Notify(EventType.PlayerDeath);
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.ItemAcquired || eventType == EventType.ItemRemoved)
        {
            RefreshEquipmentStats();
            
            // Re-broadcast stats refreshed event if needed
        }
    }
}


