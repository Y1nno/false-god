import os
import uuid

# Configuration
GUID_ENEMY_SO = "aaabbbcccdddeeefff00011122233344"
OUTPUT_DIR = r"e:\Downloads\false-god\Assets\Resources\Enemies"

if not os.path.exists(OUTPUT_DIR):
    os.makedirs(OUTPUT_DIR)

# Enemy Data
enemies = [
    {"name": "Skeleton", "hp": 12, "mana": 0, "atk": 2, "spatk": 0, "def": 1, "spdef": 0, "speed": 5, "dodge": 2.0, "crit": 5.0, "xp": 5, "special": ""},
    {"name": "Skeleton Archer", "hp": 8, "mana": 0, "atk": 4, "spatk": 0, "def": 1, "spdef": 0, "speed": 3, "dodge": 2.0, "crit": 5.0, "xp": 5, "special": "Attack Prio, reloads next round"},
    {"name": "Goblin Rogue", "hp": 8, "mana": 5, "atk": 3, "spatk": 0, "def": 0, "spdef": 0, "speed": 9, "dodge": 5.0, "crit": 8.0, "xp": 8, "special": "Steal Gold"},
    {"name": "Crypt Fiend", "hp": 12, "mana": 5, "atk": 0, "spatk": 2, "def": 2, "spdef": 2, "speed": 7, "dodge": 0.0, "crit": 3.0, "xp": 6, "special": ""},
    {"name": "Wraith", "hp": 10, "mana": 10, "atk": 2, "spatk": 0, "def": 2, "spdef": 5, "speed": 6, "dodge": 2.0, "crit": 5.0, "xp": 10, "special": "Spite: reduces player def -30%"},
    {"name": "Bone Devil", "hp": 18, "mana": 0, "atk": 5, "spatk": 0, "def": 2, "spdef": 2, "speed": 7, "dodge": 5.0, "crit": 10.0, "xp": 15, "special": ""},
    {"name": "Dark Warrior", "hp": 20, "mana": 0, "atk": 3, "spatk": 0, "def": 3, "spdef": 1, "speed": 6, "dodge": 15.0, "crit": 3.0, "xp": 18, "special": ""},
    {"name": "Imp", "hp": 10, "mana": 0, "atk": 3, "spatk": 0, "def": 2, "spdef": 1, "speed": 8, "dodge": 10.0, "crit": 8.0, "xp": 5, "special": "Passive: Chance to bleed on atk"},
    {"name": "NullMite", "hp": 5, "mana": 0, "atk": 0, "spatk": 0, "def": 0, "spdef": 0, "speed": 25, "dodge": 0.0, "crit": 0.0, "xp": 15, "special": "Runs away if not killed on first turn on atk"},
    {"name": "Heretic", "hp": 15, "mana": 5, "atk": 5, "spatk": 3, "def": 2, "spdef": 2, "speed": 10, "dodge": 5.0, "crit": 5.0, "xp": 12, "special": "Curse: Disables random equipment for 2 turns"},
    {"name": "Stone Golem", "hp": 25, "mana": 0, "atk": 4, "spatk": 0, "def": 4, "spdef": 4, "speed": 1, "dodge": 0.0, "crit": 0.0, "xp": 25, "special": ""},
    {"name": "Infected Hound", "hp": 18, "mana": 0, "atk": 4, "spatk": 2, "def": 2, "spdef": 1, "speed": 10, "dodge": 10.0, "crit": 20.0, "xp": 20, "special": "Passive: Chance to Poison"},
    {"name": "Infernal", "hp": 15, "mana": 0, "atk": 4, "spatk": 3, "def": 2, "spdef": 2, "speed": 5, "dodge": 5.0, "crit": 5.0, "xp": 25, "special": "Passive: 30% Chance to burn on atk"},
    {"name": "Cloaker", "hp": 20, "mana": 0, "atk": 3, "spatk": 0, "def": 2, "spdef": 1, "speed": 12, "dodge": 50.0, "crit": 10.0, "xp": 25, "special": ""},
    {"name": "Banshee", "hp": 18, "mana": 10, "atk": 0, "spatk": 5, "def": 2, "spdef": 3, "speed": 8, "dodge": 8.0, "crit": 7.0, "xp": 28, "special": "Haunt: reduces player atk and sp.atk -30%"},
    {"name": "Basilisk", "hp": 25, "mana": 0, "atk": 1, "spatk": 0, "def": 7, "spdef": 2, "speed": 5, "dodge": 0.0, "crit": 0.0, "xp": 20, "special": "Passive: returns 30% of dmg taken to attacker"},
    {"name": "Kobold", "hp": 10, "mana": 0, "atk": 2, "spatk": 0, "def": 2, "spdef": 2, "speed": 10, "dodge": 5.0, "crit": 5.0, "xp": 15, "special": "Passive: Blocks Phys.atk for 70% of the damage"},
    {"name": "Brute Ogre", "hp": 20, "mana": 5, "atk": 5, "spatk": 0, "def": 2, "spdef": 2, "speed": 5, "dodge": 0.0, "crit": 0.0, "xp": 25, "special": "Bash: deals 6 atk dmg 50% Chance to stun"},
    {"name": "Demon Centaur", "hp": 18, "mana": 0, "atk": 7, "spatk": 0, "def": 5, "spdef": 4, "speed": 8, "dodge": 5.0, "crit": 5.0, "xp": 25, "special": ""},
    {"name": "Mimic Chest", "hp": 12, "mana": 5, "atk": 1, "spatk": 0, "def": 2, "spdef": 2, "speed": 2, "dodge": 0.0, "crit": 0.0, "xp": 8, "special": "Devour: Instantly deals 50% of player HP if not killed on 3rd turn"},
    {"name": "Succubus", "hp": 12, "mana": 5, "atk": 3, "spatk": 0, "def": 2, "spdef": 3, "speed": 8, "dodge": 10.0, "crit": 10.0, "xp": 12, "special": "Charm: 50% chance to let the target attack itself"}
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
  EnemyName: {name}
  BaseHealth: {hp}
  BaseMana: {mana}
  BaseAtk: {atk}
  BaseSpAtk: {spatk}
  BaseDef: {defense}
  BaseSpDef: {spdef}
  BaseSpeed: {speed}
  DodgeChance: {dodge}
  CritChance: {crit}
  BaseXP: {xp}
  AvailableActionIDs:
  - 1
  GoldValue: 0
  SpecialDescription: "{special}"
"""

for enemy in enemies:
    content = asset_template.format(
        guid_script=GUID_ENEMY_SO,
        name=enemy["name"],
        hp=enemy["hp"],
        mana=enemy["mana"],
        atk=enemy["atk"],
        spatk=enemy["spatk"],
        defense=enemy["def"],
        spdef=enemy["spdef"],
        speed=enemy["speed"],
        dodge=enemy["dodge"],
        crit=enemy["crit"],
        xp=enemy["xp"],
        special=enemy["special"]
    )
    
    file_path = os.path.join(OUTPUT_DIR, enemy["name"] + ".asset")
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

print("Successfully generated all Enemy Assets in Resources/Enemies!")
