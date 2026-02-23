using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TraitWithValue))]
public class TraitWithValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty traitProp = property.FindPropertyRelative("Trait");
        SerializedProperty valueProp = property.FindPropertyRelative("Value");
        SerializedProperty cooldownTypeProp = property.FindPropertyRelative("CooldownType");
        SerializedProperty cooldownDurationProp = property.FindPropertyRelative("CooldownDuration");

        // Calculate rects
        Rect traitRect = new Rect(position.x, position.y, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);
        Rect valueRect = new Rect(position.x + position.width * 0.5f + 2, position.y, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);

        // Draw basic fields
        EditorGUI.PropertyField(traitRect, traitProp, GUIContent.none);
        
        // Only show Value if it's NOT MoveFirst
        if (traitProp.enumValueIndex != (int)EquipmentTrait.MoveFirst)
        {
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
        }

        // Conditionally draw MoveFirst properties on the next line
        if (traitProp.enumValueIndex == (int)EquipmentTrait.MoveFirst)
        {
            Rect cooldownTypeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);
            Rect cooldownDurationRect = new Rect(position.x + position.width * 0.5f + 2, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(cooldownTypeRect, cooldownTypeProp, GUIContent.none);
            EditorGUI.PropertyField(cooldownDurationRect, cooldownDurationProp, GUIContent.none);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty traitProp = property.FindPropertyRelative("Trait");
        if (traitProp.enumValueIndex == (int)EquipmentTrait.MoveFirst)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2; // Two lines
        }
        return EditorGUIUtility.singleLineHeight; // One line
    }
}
