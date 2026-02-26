using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReligionLevel))]
public class ReligionLevelDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        // 1 line when collapsed (foldout only)
        if (!property.isExpanded)
            return line;

        int lines = 1; // foldout label row is handled by OnGUI, but we count content below

        // Requirement always shown
        lines += 1; // requirement enum

        var requirementProp = property.FindPropertyRelative("requirement");
        if (requirementProp != null)
        {
            var req = (QuestType)requirementProp.enumValueIndex;
            switch (req)
            {
                case QuestType.JoinReligion:
                case QuestType.KillBoss:
                    lines += 1;
                    break;

                case QuestType.GiveItem:
                    lines += 2;
                    break;
            }
        }

        // Reward always shown
        lines += 1; // rewardType enum

        var rewardTypeProp = property.FindPropertyRelative("rewardType");
        if (rewardTypeProp != null)
        {
            var rewardType = (ReligionRewardType)rewardTypeProp.enumValueIndex;
            switch (rewardType)
            {
                case ReligionRewardType.SpecificItem:
                case ReligionRewardType.StatPoints:
                case ReligionRewardType.HealingMultiplier:
                case ReligionRewardType.AilmentImmunity:
                case ReligionRewardType.Passive:
                case ReligionRewardType.FreeRepairAtBlackSmith:
                case ReligionRewardType.UpgradeEquipment:
                case ReligionRewardType.ManaRecovery:
                    lines += 1;
                    break;
                case ReligionRewardType.StartOfCombatStatBuff:
                case ReligionRewardType.RandomItem:
                case ReligionRewardType.StartOfCombatSecondaryStatBuff:
                case ReligionRewardType.StatBuff:
                    lines += 2;
                    break;
                case ReligionRewardType.AilmentOnEnemy:
                    lines += 4;
                    break;
            }
        }

        // Total height: each line + spacing between lines
        return (lines * line) + ((lines - 1) * space) + line; // +line for foldout row
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        // Foldout row
        var row = new Rect(position.x, position.y, position.width, line);
        property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, label, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        // Move to first content row
        row.y += line + space;

        // --------------------
        // REQUIREMENT (QUEST)
        // --------------------
        var requirementProp = property.FindPropertyRelative("requirement");
        EditorGUI.PropertyField(row, requirementProp, new GUIContent("Requirement"));

        row.y += line + space;

        if (requirementProp != null)
        {
            var req = (QuestType)requirementProp.enumValueIndex;

            switch (req)
            {
                case QuestType.JoinReligion:
                {
                    break;
                }
                case QuestType.KillBoss:
                {
                    var bossProp = property.FindPropertyRelative("bossID");
                    EditorGUI.PropertyField(row, bossProp, new GUIContent("Boss ID"));
                    row.y += line + space;
                    break;
                }
                case QuestType.GiveItem:
                {
                    var itemProp = property.FindPropertyRelative("requiredItemID");
                    EditorGUI.PropertyField(row, itemProp, new GUIContent("Item ID"));
                    row.y += line + space;

                    var amtProp = property.FindPropertyRelative("requiredItemAmount");
                    EditorGUI.PropertyField(row, amtProp, new GUIContent("Amount"));
                    row.y += line + space;
                    break;
                }
            }
        }

        // --------------------
        // REWARD
        // --------------------
        var rewardTypeProp = property.FindPropertyRelative("rewardType");
        EditorGUI.PropertyField(row, rewardTypeProp, new GUIContent("Reward Type"));

        row.y += line + space;

        if (rewardTypeProp != null)
        {
            var rewardType = (ReligionRewardType)rewardTypeProp.enumValueIndex;

            switch (rewardType)
            {
                case ReligionRewardType.StartOfCombatStatBuff:
                {
                    var buffProp = property.FindPropertyRelative("startOfCombatStatBuff");
                    if (buffProp != null)
                    {
                        var statProp = buffProp.FindPropertyRelative("stat");
                        var valueProp = buffProp.FindPropertyRelative("value");

                        EditorGUI.PropertyField(row, statProp, new GUIContent("Stat"));
                        row.y += line + space;

                        EditorGUI.PropertyField(row, valueProp, new GUIContent("Stat Value"));
                        row.y += line + space;
                    }
                    else
                    {
                        EditorGUI.LabelField(row, "Missing field: startOfCombatStatBuff");
                        row.y += line + space;
                    }
                    break;
                }
                case ReligionRewardType.SpecificItem:
                {
                    var idProp = property.FindPropertyRelative("specificItemID");
                    EditorGUI.PropertyField(row, idProp, new GUIContent("Item ID"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.RandomItem:
                {
                    var slotProp = property.FindPropertyRelative("randomSlot");
                    EditorGUI.PropertyField(row, slotProp, new GUIContent("Slot"));
                    row.y += line + space;

                    var rarityProp = property.FindPropertyRelative("randomRarity");
                    EditorGUI.PropertyField(row, rarityProp, new GUIContent("Rarity"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.StartOfCombatSecondaryStatBuff:
                {
                    var buffProp = property.FindPropertyRelative("startOfCombatSecondaryStatBuff");
                    if (buffProp != null)
                    {
                        var statProp = buffProp.FindPropertyRelative("stat");
                        var valueProp = buffProp.FindPropertyRelative("value");

                        EditorGUI.PropertyField(row, statProp, new GUIContent("Stat"));
                        row.y += line + space;

                        EditorGUI.PropertyField(row, valueProp, new GUIContent("Stat Value"));
                        row.y += line + space;
                    }
                    else
                    {
                        EditorGUI.LabelField(row, "Missing field: startOfCombatSecondaryStatBuff");
                        row.y += line + space;
                    }
                    break;
                }
                case ReligionRewardType.StatPoints:
                {
                    var pointsProp = property.FindPropertyRelative("StatPointsToAllocate");
                    EditorGUI.PropertyField(row, pointsProp, new GUIContent("Stat Points"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.HealingMultiplier:
                {
                    var healingProp = property.FindPropertyRelative("HealingMultiplier");
                    EditorGUI.PropertyField(row, healingProp, new GUIContent("Healing Multiplier"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.AilmentImmunity:
                {
                    var ailmentProp = property.FindPropertyRelative("ailmentImmunity");
                    EditorGUI.PropertyField(row, ailmentProp, new GUIContent("Ailment Immunity"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.AilmentOnEnemy:
                {
                    var ailmentOnEnemyProp = property.FindPropertyRelative("ailmentOnEnemy");
                    var ailmentProp = ailmentOnEnemyProp.FindPropertyRelative("ailmentType");
                    EditorGUI.PropertyField(row, ailmentProp, new GUIContent("Ailment On Enemy"));
                    row.y += line + space;


                    var chanceProp = ailmentOnEnemyProp.FindPropertyRelative("chance");
                    EditorGUI.PropertyField(row, chanceProp, new GUIContent("Chance"));
                    row.y += line + space;

                    var maxEnemiesProp = ailmentOnEnemyProp.FindPropertyRelative("maxEnemies");
                    EditorGUI.PropertyField(row, maxEnemiesProp, new GUIContent("Max Enemies"));
                    row.y += line + space;

                    var encounterCooldownProp = ailmentOnEnemyProp.FindPropertyRelative("encounterCooldown");
                    EditorGUI.PropertyField(row, encounterCooldownProp, new GUIContent("Encounter Cooldown"));
                    row.y += line + space;

                    break;
                }
                case ReligionRewardType.Passive:
                {
                    var passiveProp = property.FindPropertyRelative("passive");
                    EditorGUI.PropertyField(row, passiveProp, new GUIContent("Passive"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.FreeRepairAtBlackSmith:
                {
                    var freeRepairsProp = property.FindPropertyRelative("FreeRepairsAtBlackSmith");
                    EditorGUI.PropertyField(row, freeRepairsProp, new GUIContent("# Free Repairs"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.UpgradeEquipment:
                {
                    var upgradeEquipmentProp = property.FindPropertyRelative("UpgradeEquipment");
                    EditorGUI.PropertyField(row, upgradeEquipmentProp, new GUIContent("Upgrade Equipment"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.ManaRecovery:
                {
                    var manaRecoveryProp = property.FindPropertyRelative("manaRecoveryPercent");
                    EditorGUI.PropertyField(row, manaRecoveryProp, new GUIContent("Mana Recovery %"));
                    row.y += line + space;
                    break;
                }
                case ReligionRewardType.StatBuff:
                {
                    var statBuffProp = property.FindPropertyRelative("statBuff");
                    var statProp = statBuffProp.FindPropertyRelative("stat");
                    EditorGUI.PropertyField(row, statProp, new GUIContent("Stat Buff"));
                    row.y += line + space;

                    var valueProp = statBuffProp.FindPropertyRelative("value");
                    EditorGUI.PropertyField(row, valueProp, new GUIContent("Value"));
                    row.y += line + space;
                    break;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}