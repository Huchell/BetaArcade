using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaterialPropertyLerp))]
public class MaterialPropertyLerpEditor : Editor {

}

[CustomPropertyDrawer(typeof(MaterialPropertyLerp.LerpData))]
public class LerpDataEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float returnValue = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            returnValue *= 5;

            switch ((MaterialPropertyLerp.LerpData.PropertyType)property.FindPropertyRelative("type").enumValueIndex)
            {
                case MaterialPropertyLerp.LerpData.PropertyType.Color: 
                case MaterialPropertyLerp.LerpData.PropertyType.Float: 
                case MaterialPropertyLerp.LerpData.PropertyType.Vector2: 
                case MaterialPropertyLerp.LerpData.PropertyType.Vector3: returnValue += EditorGUIUtility.singleLineHeight; break;
                case MaterialPropertyLerp.LerpData.PropertyType.Vector4: returnValue += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("toVector4")); break;
            }
        }

        return returnValue;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect foldoutPos = position;
        foldoutPos.height = EditorGUIUtility.singleLineHeight;

        property.isExpanded = EditorGUI.Foldout(foldoutPos, property.isExpanded, label);

        if (property.isExpanded)
        {
            Rect
                pos1 = position,
                pos2 = position,
                pos3 = position,
                pos4 = position,
                pos5 = position;

            pos1.height = pos2.height = pos3.height = pos4.height = pos5.height = EditorGUIUtility.singleLineHeight;
            pos1.y += EditorGUIUtility.singleLineHeight;
            pos2.y += EditorGUIUtility.singleLineHeight * 2;
            pos3.y += EditorGUIUtility.singleLineHeight * 3;
            pos4.y += EditorGUIUtility.singleLineHeight * 4;
            pos5.y += EditorGUIUtility.singleLineHeight * 5;

            EditorGUI.PropertyField(pos1, property.FindPropertyRelative("name"));
            EditorGUI.PropertyField(pos2, property.FindPropertyRelative("type"));
            EditorGUI.PropertyField(pos3, property.FindPropertyRelative("duration"));
            EditorGUI.PropertyField(pos4, property.FindPropertyRelative("speed"));

            switch ((MaterialPropertyLerp.LerpData.PropertyType)property.FindPropertyRelative("type").enumValueIndex)
            {
                case MaterialPropertyLerp.LerpData.PropertyType.Color: EditorGUI.PropertyField(pos5, property.FindPropertyRelative("toColor")); break;
                case MaterialPropertyLerp.LerpData.PropertyType.Float: EditorGUI.PropertyField(pos5, property.FindPropertyRelative("toFloat")); break;
                case MaterialPropertyLerp.LerpData.PropertyType.Vector2: EditorGUI.PropertyField(pos5, property.FindPropertyRelative("toVector2")); break;
                case MaterialPropertyLerp.LerpData.PropertyType.Vector3: EditorGUI.PropertyField(pos5, property.FindPropertyRelative("toVector3")); break;
                case MaterialPropertyLerp.LerpData.PropertyType.Vector4: EditorGUI.PropertyField(pos5, property.FindPropertyRelative("toVector4")); break;
            }
        }
    }
}
