using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NameAttribute : PropertyAttribute
{
    public string name;

    public NameAttribute(string name)
    {
        this.name = name;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NameAttribute))]
public class NameAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent((attribute as NameAttribute).name));
    }
}
#endif
