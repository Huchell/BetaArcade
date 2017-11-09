using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute {

    public bool active;

    public ReadOnlyAttribute()
    {
        active = true;
    }

    public ReadOnlyAttribute(bool active)
    {
        this.active = active;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadOnlyAttribute ro = attribute as ReadOnlyAttribute;

        GUI.enabled = !ro.active;

        EditorGUI.PropertyField(position, property, label);

        GUI.enabled = true;
    }
}
#endif