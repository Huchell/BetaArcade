using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spill))]
public class SpillEditor : Editor {

    private readonly GUIContent materialTitle = new GUIContent("Material");
    private readonly GUIContent debugTitle = new GUIContent("Debug");
    private readonly GUIContent lerpSpillButton = new GUIContent("Lerp Spill");

    private SerializedProperty
        sp_NormalMaterial,
        sp_InertMaterial,
        sp_debug,
        sp_lerpValue;

    private Spill castedTarget;

    private void OnEnable()
    {
        sp_NormalMaterial = serializedObject.FindProperty("NormalMaterial");
        sp_InertMaterial = serializedObject.FindProperty("InertMaterial");
        sp_debug = serializedObject.FindProperty("debug");
        sp_lerpValue = serializedObject.FindProperty("lerpValue");

        castedTarget = (target as Spill);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        MaterialGUI();

        EditorGUILayout.Space();

        DebugGUI();

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button(lerpSpillButton, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
        {
            sp_lerpValue.floatValue = 0;
            castedTarget.MakeSpillInert();
        }

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void MaterialGUI()
    {
        EditorGUILayout.LabelField(materialTitle, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        sp_NormalMaterial.objectReferenceValue = EditorGUILayout.ObjectField(
            new GUIContent(sp_NormalMaterial.displayName),
            sp_NormalMaterial.objectReferenceValue,
            typeof(Material),
            false
            );

        if (EditorGUI.EndChangeCheck())
        {
            castedTarget.GetComponent<MeshRenderer>().material = sp_NormalMaterial.objectReferenceValue as Material;
        }

        sp_InertMaterial.objectReferenceValue = EditorGUILayout.ObjectField(
            new GUIContent(sp_InertMaterial.displayName),
            sp_InertMaterial.objectReferenceValue,
            typeof(Material),
            false
            );
    }

    void DebugGUI()
    {
        EditorGUILayout.LabelField(debugTitle, EditorStyles.boldLabel);

        sp_debug.boolValue = EditorGUILayout.Toggle(sp_debug.displayName, sp_debug.boolValue);

        GUI.enabled = sp_debug.boolValue && !Application.isPlaying;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(sp_lerpValue);

        if (EditorGUI.EndChangeCheck())
        {
            castedTarget.UpdateMaterial(sp_lerpValue.floatValue);
        }

        GUI.enabled = true;
    }
}
