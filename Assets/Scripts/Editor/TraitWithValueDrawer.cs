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

        EquipmentTrait trait = (EquipmentTrait)traitProp.enumValueIndex;

        // Calculate rects
        Rect traitRect = new Rect(position.x, position.y, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);
        Rect valueRect = new Rect(position.x + position.width * 0.5f + 2, position.y, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);

        // Draw trait dropdown
        EditorGUI.PropertyField(traitRect, traitProp, GUIContent.none);

        // Determine how to draw the Value field
        if (IsBooleanTrait(trait))
        {
            // Do not draw Value field
        }
        else if (IsPercentageTrait(trait))
        {
            // Draw as Slider
            valueProp.intValue = EditorGUI.IntSlider(valueRect, valueProp.intValue, 0, 100);
        }
        else
        {
            // Draw as normal IntField
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
        }

        // Conditionally draw MoveFirst properties on the next line
        if (trait == EquipmentTrait.MoveFirst)
        {
            Rect cooldownTypeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);
            Rect cooldownDurationRect = new Rect(position.x + position.width * 0.5f + 2, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f - 2, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(cooldownTypeRect, cooldownTypeProp, GUIContent.none);
            EditorGUI.PropertyField(cooldownDurationRect, cooldownDurationProp, GUIContent.none);
        }

        EditorGUI.EndProperty();
    }

    private bool IsBooleanTrait(EquipmentTrait trait)
    {
        return trait == EquipmentTrait.MoveFirst || 
               trait == EquipmentTrait.TrueStrike || 
               trait == EquipmentTrait.ComboStrike || 
               trait == EquipmentTrait.HealAfterFirstDamage;
    }

    private bool IsPercentageTrait(EquipmentTrait trait)
    {
        return trait == EquipmentTrait.XPBoost ||
               trait == EquipmentTrait.GlobalDamageReduction ||
               trait == EquipmentTrait.CounterChance ||
               trait == EquipmentTrait.SpellReflect ||
               trait == EquipmentTrait.DoubleStrike ||
               trait == EquipmentTrait.PoisonHit ||
               trait == EquipmentTrait.BurnHit ||
               trait == EquipmentTrait.FreezeHit ||
               trait == EquipmentTrait.WeakenHit ||
               trait == EquipmentTrait.SplashDamage;
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
