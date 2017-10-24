using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChoppingBoard))]
public class ChoppingBoardEditor : Editor {

    SerializedProperty
        sp_KnifeTransform,
        sp_KnifeYOffset,
        sp_KnifeSpeed,
        sp_KnifeDistanceTolerence,
        sp_ShakeTime,
        sp_ShakeAmplitude,
        sp_CuttingSpeed,
        sp_Cooldown,
        sp_Target;

    private readonly GUIContent knifeContent = new GUIContent("Knife");
    private readonly GUIContent shakeTitleContent = new GUIContent("Shake");

    private void OnEnable()
    {
        sp_KnifeTransform = GetProperty("m_KnifeTransform");
        sp_KnifeYOffset = GetProperty("m_KnifeYOffset");
        sp_KnifeSpeed = GetProperty("m_KnifeSpeed");
        sp_KnifeDistanceTolerence = GetProperty("m_KnifeDistanceTolerence");
        sp_ShakeTime = GetProperty("m_ShakeTime");
        sp_ShakeAmplitude = GetProperty("m_ShakeAmplitude");
        sp_CuttingSpeed = GetProperty("m_CuttingSpeed");
        sp_Cooldown = GetProperty("m_CooldownTime");
        sp_Target = GetProperty("m_Target");
    }
    SerializedProperty GetProperty(string property)
    {
        SerializedProperty prop = null;

        try
        {
            prop = serializedObject.FindProperty(property);
        }
        catch
        {
            Debug.LogError("Serialized Object doesn't have a property of: " + property);
        }

        return prop;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Knife Group
        KnifeGroup();

        // Shake group
        ShakeGroup();

        // Cutting speed
        EditorGUILayout.PropertyField(sp_CuttingSpeed);
        EditorGUILayout.PropertyField(sp_Cooldown);

        // Debug
        DebugGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void KnifeGroup()
    {
        sp_KnifeTransform.isExpanded = EditorGUILayout.Foldout(sp_KnifeTransform.isExpanded, knifeContent);

        if (sp_KnifeTransform.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(sp_KnifeTransform, new GUIContent("Transform"));
            EditorGUILayout.PropertyField(sp_KnifeYOffset, new GUIContent("Y Offset"));
            EditorGUILayout.PropertyField(sp_KnifeSpeed, new GUIContent("Tracking Speed"));
            EditorGUILayout.PropertyField(sp_KnifeDistanceTolerence, new GUIContent("Distance Tolerence"));

            EditorGUI.indentLevel--;
        }
    }

    void ShakeGroup()
    {
        if (sp_ShakeTime.isExpanded = EditorGUILayout.Foldout(sp_ShakeTime.isExpanded, shakeTitleContent))
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(sp_ShakeTime, new GUIContent("Time"));
            EditorGUILayout.PropertyField(sp_ShakeAmplitude, new GUIContent("Amplitude"));

            EditorGUI.indentLevel--;
        }
    }

    void DebugGroup()
    {
        EditorGUILayout.PropertyField(sp_Target);
    }
}
