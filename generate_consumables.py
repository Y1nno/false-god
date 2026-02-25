import os

# Base properties for the generated consumables
consumables_data = [
    {"Name": "Health Potion", "ID": 101, "Description": "A vial of red liquid that restores health.", "EffectType": 0, "Duration": 0, "Amounts": [50, 100, 200]},
    {"Name": "Mana Potion", "ID": 102, "Description": "A vial of blue liquid that restores mana.", "EffectType": 2, "Duration": 0, "Amounts": [30, 60, 120]}
]

# Note: EffectType 0 is FlatHP, EffectType 2 is FlatMana based on the ConsumableEffectType enum parsing.
base_template = """%YAML 1.1
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
  m_Script: {{fileID: 11500000, guid: 4ff7842cffcc4ed44bd0ea55822369fc, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: Assembly-CSharp::Consumable
  ItemID: {item_id}
  ItemName: {name}
  ItemIcon: {{fileID: 0}}
  Description: {description}
  Effects:
  - Type: {effect_type}
    TargetStat: 0
    Amount:
    - {amount_tier1}
    - {amount_tier2}
    - {amount_tier3}
    Duration: {duration}
"""

output_dir = r"e:\Downloads\false-god\Assets\Scripts\ScriptableObjects\Items\Consumables"
# Ensure directory exists just in case
os.makedirs(output_dir, exist_ok=True)

for item in consumables_data:
    yaml_content = base_template.format(
        name=item["Name"],
        item_id=item["ID"],
        description=item["Description"],
        effect_type=item["EffectType"],
        amount_tier1=item["Amounts"][0],
        amount_tier2=item["Amounts"][1],
        amount_tier3=item["Amounts"][2],
        duration=item["Duration"]
    )
    
    file_path = os.path.join(output_dir, f"{item['Name']}.asset")
    with open(file_path, 'w') as f:
        f.write(yaml_content)

print(f"Consumables generated successfully to {output_dir}")
