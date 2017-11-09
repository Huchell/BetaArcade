using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMax))]
public class MinMaxEditor : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty sp_min = property.FindPropertyRelative("_min");
        SerializedProperty sp_max = property.FindPropertyRelative("_max");

        Rect halfWidthPosition = EditorGUI.PrefixLabel(position, label);
        halfWidthPosition.width /= 2;

        EditorGUIUtility.labelWidth = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.CalcSize(new GUIContent(sp_min.displayName)).x + 5;
        EditorGUI.PropertyField(halfWidthPosition, sp_min);

        EditorGUIUtility.labelWidth = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.CalcSize(new GUIContent(sp_max.displayName)).x + 5;
        halfWidthPosition.x += halfWidthPosition.width;
        EditorGUI.PropertyField(halfWidthPosition, sp_max);

        EditorGUI.EndProperty();
    }
}
