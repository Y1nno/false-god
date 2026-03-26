using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class RunSaveData
{
    public bool IsActive;
    public int Floor;
    public int EncounterIndex;
    public int Level;
    public int CurrentXP;
    public int XPThreshold;
    public int Gold;
    public int Score;
    public int STR;
    public int DEX;
    public int INT;
    public int SPD;
    public int StatPoints;
    public int CurrentHP;
    public int CurrentMana;
    public EncounterType CurrentEncounterType;
    public string CurrentTrapSOID;
    public List<string> CurrentEnemyIDs = new List<string>();
    public List<int> CurrentEnemyLevels = new List<int>();
    public string CurrentReligion;
    public int FaithLevel;
    public List<SerializableItem> EquippedItems = new List<SerializableItem>();
    public List<SerializableItem> InventoryItems = new List<SerializableItem>();
    public List<string> LearnedSpellIDs = new List<string>();
    public int EnemiesKilled;
    public List<string> EquippedRiteIDs = new List<string>();
    public List<SerializableRiteState> RiteStates = new List<SerializableRiteState>();
    
    // Current Run Trackers (to check for unlocks on save)
    public int RunGoldSpent;
    public int RunConsumablesUsed;
    public bool RunJoinedReligion;

    // Dungeon & Encounter State
    public RoomSize CurrentRoomSize;
    public int EncountersInCurrentFloor;
    public int TotalEncountersResolved;
    public bool ReligionPending;
    public bool ShopPending;
    public int EncountersSinceLastEscalation;
    public float TreasureChance, TrapChance, FountainChance, ShrineChance;

    // Shop State
    public List<SerializableShopItem> ShopInventory = new List<SerializableShopItem>();

    // Combat Detail State
    public int CombatTurn;
    public List<SerializableAilment> PlayerAilments = new List<SerializableAilment>();
    public List<List<SerializableAilment>> EnemyAilments = new List<List<SerializableAilment>>();
    public List<SerializableActiveEffect> PlayerActiveEffects = new List<SerializableActiveEffect>();
    public List<List<SerializableActiveEffect>> EnemyActiveEffects = new List<List<SerializableActiveEffect>>();

    // Permanent Buffs/Modifiers
    public int BonusDamage;
    public float DamageMultiplier;
    public bool CanUseHealthAsMana;
    public int DiceRerollCount;
    public string CurrentQuestStatus;
}

[Serializable]
public class SerializableShopItem
{
    public string ID;
    public string DisplayName;
    public int Price;
    public Rarity Rarity;
    public bool Sold;
}

[Serializable]
public class SerializableAilment
{
    public string AilmentType; // Burn, Poison, etc
    public int Stacks;
    public int Duration;
}

[Serializable]
public class MetaSaveData
{
    public List<string> UnlockedRiteIDs = new List<string>();
    public List<string> ProgressFlags = new List<string>();
    public int TotalScore;
    public int HighestFloor;
    public int BaseRitePoints;
    
    // Detailed Trackers
    public int TotalDeaths;
    public int TotalSpellsCast;
    public int TotalMaxDiceRolls;
    public List<string> BossesDefeated = new List<string>();
    public List<string> ReligionJoinedEver = new List<string>();
    public List<string> EnemyKillKeys = new List<string>();
    public List<int> EnemyKillValues = new List<int>();
    
    public bool Spent1000Gold;
    public bool Used50Consumables;
    public string StartingRelicID; // For Afterbirth Rite
}

[Serializable]
public class SerializableRiteState
{
    public string RiteID;
    public int Cooldown;
    public bool IsDestroyed;
}

[Serializable]
public class SerializableActiveEffect
{
    public string EffectType;
    public float ModifiedAmount;
    public int RoundsRemaining;
    public string TargetStat;
    public string DurationType;
}

[Serializable]
public class SerializableItem
{
    public string ItemID;
    public string ItemType; // "Equipment", "Consumable", "Material", "Key", "Relic"
    public int Quantity;
    public int Tier;
    public int Durability;
    public int UpgradeLevel;
    public int RemainingUses;
    public int BasePhysDef, BaseSpecDef, BasePhysAtk, BaseSpecAtk;
    public int BaseSTR, BaseDEX, BaseINT, BaseSPD;
    public int BaseCrit, BaseBlock, BaseBlockAmt, BaseDodge;
    public List<ModifierInstance> Prefixes = new List<ModifierInstance>();
    public List<ModifierInstance> Suffixes = new List<ModifierInstance>();
    public EquipmentSlot EquippedSlot;
}

public class SaveManager : GameModule
{
    private string RunSavePath => Path.Combine(Application.persistentDataPath, "run_save.json");
    private string MetaSavePath => Path.Combine(Application.persistentDataPath, "meta_save.json");

    public override void AttachDefaultObservers()
    {
        // Initial load on startup
        LoadMeta();
    }

    public void SaveRun()
    {
        RunSaveData data = new RunSaveData();
        data.IsActive = true;

        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        if (dm != null)
        {
            data.Floor = dm.CurrentDungeonFloor;
            data.EncounterIndex = dm.CurrentEncounterIndex;
        }

        XPManager xm = RunManager.Instance.GetService<XPManager>();
        if (xm != null)
        {
            data.Level = xm.level;
            data.CurrentXP = xm.currentXP;
            data.XPThreshold = xm.xpThresholdForLevelUp;
        }

        EconomyManager em = RunManager.Instance.GetService<EconomyManager>();
        if (em != null)
        {
            data.Gold = em.GetCurrentGold();
        }

        ScoreManager scm = RunManager.Instance.GetService<ScoreManager>();
        if (scm != null)
        {
            data.Score = scm.CurrentScore;
        }

        PlayerManager pm = RunManager.Instance.GetService<PlayerManager>();
        if (pm != null)
        {
            data.CurrentHP = pm.Health.CurrentValue;
            data.CurrentMana = pm.Mana.CurrentValue;
            data.STR = Mathf.Max(1, pm.PlayerStats.STR);
            data.DEX = Mathf.Max(1, pm.PlayerStats.DEX);
            data.INT = Mathf.Max(1, pm.PlayerStats.INT);
            data.SPD = Mathf.Max(1, pm.PlayerStats.SPD);
            data.StatPoints = pm.PlayerStats.AvailableStatPoints;
            
            data.BonusDamage = pm.BonusDamage;
            data.DamageMultiplier = pm.DamageMultiplier;
            data.CanUseHealthAsMana = pm.CanUseHealthAsMana;
            data.DiceRerollCount = pm.DiceRerollCount;
        }

        ReligionManager religM = RunManager.Instance.GetService<ReligionManager>();
        if (religM != null && religM.CurrentReligion != null)
        {
            data.CurrentReligion = religM.CurrentReligion.ReligionID;
            data.FaithLevel = religM.CurrentReligion.CurrentFaithLevel;
            data.CurrentQuestStatus = religM.CurrentReligion.CurrentQuestStatus.ToString();
        }

        if (dm != null)
        {
            data.Floor = dm.CurrentDungeonFloor;
            data.EncounterIndex = dm.CurrentEncounterIndex;
            data.CurrentRoomSize = dm.CurrentRoomSize;
            data.EncountersInCurrentFloor = dm.EncountersInCurrentFloor;
        }

        EncounterManager encm = RunManager.Instance.GetService<EncounterManager>();
        if (encm != null)
        {
            data.CurrentEncounterType = encm.GetCurrentEncounterType();
            data.CurrentTrapSOID = encm.GetCurrentTrapSOID();
            data.TotalEncountersResolved = encm.TotalEncountersResolved;
            data.ReligionPending = encm.ReligionPending;
            data.ShopPending = encm.ShopPending;
            data.EncountersSinceLastEscalation = encm.EncountersSinceLastEscalation;
            data.TreasureChance = encm.TreasureChance;
            data.TrapChance = encm.TrapChance;
            data.FountainChance = encm.FountainChance;
            data.ShrineChance = encm.ShrineChance;

            // Handle Shop inventory
            Encounter current = encm.GetCurrentEncounter();
            if (current is BlacksmithEncounter be) data.ShopInventory = be.GetSerializedInventory();
            else if (current is MerchantEncounter me) data.ShopInventory = me.GetSerializedInventory();
        }

        CombatManager cmanager = RunManager.Instance.GetService<CombatManager>();
        if (cmanager != null)
        {
            data.CombatTurn = cmanager.CurrentBattle != null ? cmanager.CurrentBattle.TurnCount : 1;
            data.PlayerAilments = ToSerializable(cmanager.Pcm.ActiveAilments);
            data.PlayerActiveEffects = cmanager.Pcm.GetActiveEffects();

            if (cmanager.CurrentBattle != null)
            {
                foreach (var combatant in cmanager.CurrentBattle.combatants)
                {
                    if (combatant is Enemy enemy && enemy.EnemyData != null)
                    {
                        data.CurrentEnemyIDs.Add(enemy.EnemyData.name);
                        data.CurrentEnemyLevels.Add(enemy.Level);
                    }
                }
                
                data.EnemyAilments = new List<List<SerializableAilment>>();
                data.EnemyActiveEffects = new List<List<SerializableActiveEffect>>();
                foreach (var enemy in cmanager.CurrentBattle.GetEnemies())
                {
                    data.EnemyAilments.Add(ToSerializable(enemy.ActiveAilments));
                    data.EnemyActiveEffects.Add(enemy.GetActiveEffects());
                }
            }
        }

        ReligionManager rm = RunManager.Instance.GetService<ReligionManager>();
        if (rm != null && rm.CurrentReligion != null)
        {
            data.CurrentReligion = rm.CurrentReligion.ReligionID;
            data.FaithLevel = rm.CurrentReligion.CurrentFaithLevel;
        }

        RelicManager relicm = RunManager.Instance.GetService<RelicManager>();
        if (relicm != null)
        {
            data.EnemiesKilled = relicm.GetEnemiesKilled();
        }

        SpellManager spellm = RunManager.Instance.GetService<SpellManager>();
        if (spellm != null)
        {
            data.LearnedSpellIDs = spellm.LearnedSpells.Select(s => s.name).ToList(); // Based on Resource name
        }

        EquipmentManager eqm = RunManager.Instance.GetService<EquipmentManager>();
        if (eqm != null)
        {
            foreach (var kvp in eqm.EquippedItems)
            {
                data.EquippedItems.Add(ToSerializable(kvp.Value, kvp.Key));
            }
        }

        InventoryManager invm = RunManager.Instance.GetService<InventoryManager>();
        if (invm != null)
        {
            foreach (var item in invm.UnEquippedItems)
            {
                data.InventoryItems.Add(ToSerializable(item));
            }
        }

        RiteManager riteman = RunManager.Instance.GetService<RiteManager>();
        if (riteman != null)
        {
            foreach (var rite in riteman.ActiveRites.Values)
            {
                data.EquippedRiteIDs.Add(rite.RiteID);
                data.RiteStates.Add(new SerializableRiteState 
                { 
                    RiteID = rite.RiteID, 
                    Cooldown = rite.CurrentCooldown, 
                    IsDestroyed = rite.IsDestroyed 
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(RunSavePath, json);
        
        // Finalize Meta Score as well on every save
        SaveMeta();
        
        Debug.Log($"Run saved to {RunSavePath}");
    }

    public void LoadRun()
    {
        if (!File.Exists(RunSavePath)) return;

        string json = File.ReadAllText(RunSavePath);
        RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);

        if (!data.IsActive) return;

        RunManager rm = RunManager.Instance;
        
        rm.GetService<DungeonManager>()?.RestoreState(data.Floor, data.EncounterIndex, data.CurrentRoomSize, data.EncountersInCurrentFloor);
        rm.GetService<XPManager>()?.RestoreState(data.Level, data.CurrentXP, data.XPThreshold);
        rm.GetService<EconomyManager>()?.RestoreState(data.Gold);
        rm.GetService<ScoreManager>()?.RestoreState(data.Score);
        rm.GetService<PlayerManager>()?.PlayerStats.RestoreState(data.STR, data.DEX, data.INT, data.SPD, data.StatPoints);
        
        PlayerManager pm = rm.GetService<PlayerManager>();
        if (pm != null)
        {
            pm.RestoreState(data.CurrentHP, data.CurrentMana);
            pm.BonusDamage = data.BonusDamage;
            pm.DamageMultiplier = data.DamageMultiplier;
            pm.CanUseHealthAsMana = data.CanUseHealthAsMana;
            pm.DiceRerollCount = data.DiceRerollCount;
        }

        rm.GetService<ReligionManager>()?.RestoreState(data.CurrentReligion, data.FaithLevel, data.CurrentQuestStatus);
        
        rm.GetService<EquipmentManager>()?.RestoreState(data.EquippedItems);
        rm.GetService<InventoryManager>()?.RestoreState(data.InventoryItems);
        rm.GetService<RelicManager>()?.RestoreState(data.EnemiesKilled);
        rm.GetService<SpellManager>()?.RestoreState(data.LearnedSpellIDs);
        
        RiteManager ritem = rm.GetService<RiteManager>();
        if (ritem != null)
        {
            foreach (var riteID in data.EquippedRiteIDs)
            {
                if (Enum.TryParse(riteID, out RiteType type))
                {
                    ritem.EquipRite(type);
                    
                    // Restore cooldown/destroyed state
                    var state = data.RiteStates.Find(s => s.RiteID == riteID);
                    if (state != null)
                    {
                        var rite = ritem.GetRite(type);
                        if (rite != null)
                        {
                            rite.CurrentCooldown = state.Cooldown;
                            rite.IsDestroyed = state.IsDestroyed;
                        }
                    }
                }
            }
        }
        
        // Restore Encounter last
        rm.GetService<EncounterManager>()?.RestoreState(data);

        // After encounter is restored, we might need to apply ailments/effects to combatants if in battle
        CombatManager cm = rm.GetService<CombatManager>();
        if (cm != null && cm.CurrentBattle != null)
        {
            cm.Pcm.RestoreActiveEffects(data.PlayerActiveEffects);
            var enemies = cm.CurrentBattle.GetEnemies();
            for (int i = 0; i < enemies.Count; i++)
            {
                if (data.EnemyActiveEffects != null && i < data.EnemyActiveEffects.Count)
                {
                    enemies[i].RestoreActiveEffects(data.EnemyActiveEffects[i]);
                }
            }
        }
        
        // Re-refresh stats after everything is loaded
        rm.GetService<PlayerManager>()?.RefreshEquipmentStats();

        Debug.Log($"Run loaded from {RunSavePath} - Stats Restored: {data.STR}/{data.DEX}/{data.INT}/{data.SPD}, Score: {data.Score}");
    }

    public void SaveMeta()
    {
        MetaSaveData data = new MetaSaveData();
        RiteManager ritem = RunManager.Instance.GetService<RiteManager>();
        if (ritem != null)
        {
            data.UnlockedRiteIDs = ritem.GetUnlockedRiteIDs();
        }
        
        ScoreManager sm = RunManager.Instance.GetService<ScoreManager>();
        if (sm != null)
        {
            data.TotalScore = sm.TotalScore;
        }

        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        if (dm != null)
        {
            data.HighestFloor = dm.HighestFloorReached;
        }

        if (ritem != null)
        {
            data.UnlockedRiteIDs = ritem.GetUnlockedRiteIDs();
            data.BaseRitePoints = ritem.BaseRitePoints;
            
            data.TotalDeaths = ritem.TotalDeaths;
            data.TotalSpellsCast = ritem.TotalSpellsCast;
            data.TotalMaxDiceRolls = ritem.TotalMaxDiceRolls;
            data.BossesDefeated = new List<string>(ritem.BossesDefeated);
            data.ReligionJoinedEver = new List<string>(ritem.ReligionsJoined);
            data.Spent1000Gold = ritem.Spent1000Gold;
            data.Used50Consumables = ritem.Used50Consumables;
            data.StartingRelicID = ritem.StartingRelicID;
            
            data.EnemyKillKeys = new List<string>(ritem.KillsByEnemyID.Keys);
            data.EnemyKillValues = new List<int>(ritem.KillsByEnemyID.Values);
        }
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(MetaSavePath, json);
        Debug.Log($"Meta data saved to {MetaSavePath}");
    }

    public void LoadMeta()
    {
        if (!File.Exists(MetaSavePath)) return;
        string json = File.ReadAllText(MetaSavePath);
        MetaSaveData data = JsonUtility.FromJson<MetaSaveData>(json);
        
        RiteManager ritem = RunManager.Instance.GetService<RiteManager>();
        if (ritem != null)
        {
            ritem.SetUnlockedRiteIDs(data.UnlockedRiteIDs);
            ritem.BaseRitePoints = data.BaseRitePoints;
            
            ritem.TotalDeaths = data.TotalDeaths;
            ritem.TotalSpellsCast = data.TotalSpellsCast;
            ritem.TotalMaxDiceRolls = data.TotalMaxDiceRolls;
            ritem.BossesDefeated = new HashSet<string>(data.BossesDefeated);
            ritem.ReligionsJoined = new HashSet<string>(data.ReligionJoinedEver);
            ritem.Spent1000Gold = data.Spent1000Gold;
            ritem.Used50Consumables = data.Used50Consumables;
            ritem.StartingRelicID = data.StartingRelicID;
            
            ritem.KillsByEnemyID.Clear();
            for (int i = 0; i < data.EnemyKillKeys.Count && i < data.EnemyKillValues.Count; i++)
            {
                ritem.KillsByEnemyID[data.EnemyKillKeys[i]] = data.EnemyKillValues[i];
            }
        }

        ScoreManager sm = RunManager.Instance.GetService<ScoreManager>();
        if (sm != null)
        {
            sm.RestoreMetaState(data.TotalScore);
        }

        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        if (dm != null)
        {
            dm.RestoreMetaState(data.HighestFloor);
        }
    }

    public void ClearRunSave()
    {
        if (File.Exists(RunSavePath))
        {
            File.Delete(RunSavePath);
            Debug.Log("Run save cleared.");
        }
    }

    public bool HasRunSave()
    {
        return File.Exists(RunSavePath);
    }

    public void NukeAllData()
    {
        ClearRunSave();
        if (File.Exists(MetaSavePath))
        {
            File.Delete(MetaSavePath);
            Debug.Log("Meta save cleared.");
        }
        TextOutputter.Instance.OutputText("<color=red>ALL PROGRESS ERASED.</color>");
    }

    private SerializableItem ToSerializable(Item item, EquipmentSlot slot = EquipmentSlot.Head)
    {
        SerializableItem sData = new SerializableItem { ItemID = "unknown", ItemType = "None" };

        if (item is Equipment eq)
        {
            sData.ItemID = eq.BaseData.ItemID;
            sData.ItemType = "Equipment";
            sData.Durability = eq.CurrentDurability;
            sData.UpgradeLevel = eq.UpgradeLevel;
            sData.Prefixes = new List<ModifierInstance>(eq.Prefixes);
            sData.Suffixes = new List<ModifierInstance>(eq.Suffixes);
            sData.EquippedSlot = slot;

            sData.BasePhysDef = eq._basePhysicalDefense;
            sData.BaseSpecDef = eq._baseSpecialDefense;
            sData.BasePhysAtk = eq._basePhysicalAttack;
            sData.BaseSpecAtk = eq._baseSpecialAttack;
            sData.BaseSTR = eq._baseSTR;
            sData.BaseDEX = eq._baseDEX;
            sData.BaseINT = eq._baseINT;
            sData.BaseSPD = eq._baseSPD;
            sData.BaseCrit = eq._baseCritChance;
            sData.BaseBlock = eq._baseBlockChance;
            sData.BaseBlockAmt = eq._baseBlockAmount;
            sData.BaseDodge = eq._baseDodgeChance;
        }
        else if (item is ConsumableInstance con)
        {
            sData.ItemID = con.BaseData.ItemID;
            sData.ItemType = "Consumable";
            sData.Tier = con.Tier;
        }
        else if (item is MaterialInstance mat)
        {
            sData.ItemID = mat.BaseData.ItemID;
            sData.ItemType = "Material";
            sData.Quantity = mat.Quantity;
        }
        else if (item is KeyInstance key)
        {
            sData.ItemID = key.BaseData.ItemID;
            sData.ItemType = "Key";
            sData.Quantity = key.Quantity;
            sData.RemainingUses = key.RemainingUses;
        }
        else if (item is RelicInstance rel)
        {
            sData.ItemID = rel.BaseData.ItemID;
            sData.ItemType = "Relic";
        }

        return sData;
    }

    private List<SerializableAilment> ToSerializable(List<Ailment> ailments)
    {
        var list = new List<SerializableAilment>();
        foreach (var a in ailments)
        {
            list.Add(new SerializableAilment { AilmentType = a.Type.ToString(), Stacks = a.Stacks, Duration = a.RoundsRemaining });
        }
        return list;
    }
}
