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
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}