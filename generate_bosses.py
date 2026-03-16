import os
import uuid

# Configuration
GUID_BOSS_SO = "b055e55e5b055e5b055e5b055e5b055e" # From BossSO.cs.meta
OUTPUT_DIR = r"e:\Downloads\false-god\Assets\Resources\Bosses"

if not os.path.exists(OUTPUT_DIR):
    os.makedirs(OUTPUT_DIR)

# Boss Data
bosses = [
    {
        "name": "Gorvath",
        "hp": 65, "mana": 10, "str": 5, "dex": 5, "int": 6, "spd": 10,
        "dodge": 5.0, "crit": 8.0, "xp": 35,
        "drops": ["Rotten Flesh", "Skeleton Key"],
        "action": 101, # Placeholder for Rotten Cleaver
        "passive": "Heal 50% when HP < 20%"
    },
    {
        "name": "Molech",
        "hp": 58, "mana": 10, "str": 8, "dex": 5, "int": 0, "spd": 15,
        "dodge": 10.0, "crit": 15.0, "xp": 35,
        "drops": ["Demonic Sword", "Skeleton Key"],
        "action": 102, # Placeholder for Demonic Sacrifice
        "passive": "30% counter chance"
    },
    {
        "name": "Vlad",
        "hp": 52, "mana": 30, "str": 5, "dex": 5, "int": 0, "spd": 20,
        "dodge": 15.0, "crit": 12.0, "xp": 35,
        "drops": ["Vladmir's Gift", "Skeleton Key"],
        "action": 103, # Placeholder for Blood Drain
        "passive": "Vampirism (Steal 5% Max HP/MP)"
    },
    {
        "name": "Fallen Saint",
        "hp": 125, "mana": 20, "str": 15, "dex": 5, "int": 0, "spd": 26,
        "dodge": 20.0, "crit": 15.0, "xp": 80,
        "drops": ["Gloves of Saint"],
        "action": 104, # Placeholder for Divine Shield
        "passive": "Aura of the Fallen (-20% enemy damage)"
    }
]

asset_template = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid_script}, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
  BossName: {name}
  BaseHP: {hp}
  BaseMana: {mana}
  STR: {str}
  DEX: {dex}
  INT: {int}
  SPD: {spd}
  DodgeChance: {dodge}
  CritChance: {crit}
  XP: {xp}
  DropIDs:
{drops}
  SpecialActionID: {action}
  PassiveDescription: "{passive}"
"""

for boss in bosses:
    drop_lines = "\n".join([f"  - {d}" for d in boss["drops"]])
    content = asset_template.format(
        guid_script=GUID_BOSS_SO,
        name=boss["name"],
        hp=boss["hp"],
        mana=boss["mana"],
        str=boss["str"],
        dex=boss["dex"],
        int=boss["int"],
        spd=boss["spd"],
        dodge=boss["dodge"],
        crit=boss["crit"],
        xp=boss["xp"],
        drops=drop_lines,
        action=boss["action"],
        passive=boss["passive"]
    )
    
    file_path = os.path.join(OUTPUT_DIR, boss["name"] + ".asset")
    with open(file_path, "w") as f:
        f.write(content)
    
    # Generate meta file
    meta_path = file_path + ".meta"
    with open(meta_path, "w") as f:
        f.write(f"""fileFormatVersion: 2
guid: {uuid.uuid4().hex}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""")

print("Successfully generated boss assets.")
