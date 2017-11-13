using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChoppingBoard))]
public class ChoppingBoardEditor : Editor {

    SerializedProperty
        sp_Knives,
        sp_ChopOnStart,
        sp_UpDelay, sp_HoldDelay, sp_DownDelay, sp_ChoppedDelay,
        sp_KnifeObjectPrefab;

    bool showSettings;
    Transform knifeHolder;
    ChoppingBoard cp_target;

    private void OnEnable()
    {
        sp_Knives = serializedObject.FindProperty("knives");
        sp_ChopOnStart = serializedObject.FindProperty("m_ChopOnStart");

        sp_UpDelay = serializedObject.FindProperty("m_upDelay");
        sp_HoldDelay = serializedObject.FindProperty("m_holdDelay");
        sp_DownDelay = serializedObject.FindProperty("m_downDelay");
        sp_ChoppedDelay = serializedObject.FindProperty("m_choppedDelay");

        sp_KnifeObjectPrefab = serializedObject.FindProperty("knifeObjectPrefab");

        cp_target = target as ChoppingBoard;

        knifeHolder = cp_target.transform.Find("KnifeHolder");

        if (!knifeHolder)
        {
            GameObject gm = new GameObject("knifeHolder");
            gm.transform.SetParent(cp_target.transform);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        #region Top Bar
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(sp_Knives.displayName);

        EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth / 6));

        if (GUILayout.Button(new GUIContent("+")))
        {
            cp_target.AddKnife();
            serializedObject.Update();
        }

        GUI.enabled = sp_Knives.arraySize > 0;

        if (GUILayout.Button(new GUIContent("-")))
        {
            cp_target.RemoveKnife(sp_Knives.arraySize - 1);
            serializedObject.Update();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        #endregion

        #region Knives Array
        for (int i = 0; i < sp_Knives.arraySize; i++)
        {
            SerializedProperty prop = sp_Knives.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, new GUIContent("Knife " + i));

            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth / 6));

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("Chop"))
            {
                cp_target.ChopKnife(i);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (prop.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(prop);

                EditorGUI.indentLevel--;
            }
        }

        #endregion

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Chop"))
        {
            cp_target.ChopAllKnives();
        }

        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Settings"))
        {
            showSettings = !showSettings;
        }

        if (showSettings)
        {
            sp_ChopOnStart.boolValue = EditorGUILayout.ToggleLeft(sp_ChopOnStart.displayName, sp_ChopOnStart.boolValue);

            EditorGUILayout.PropertyField(sp_UpDelay);
            EditorGUILayout.PropertyField(sp_HoldDelay);
            EditorGUILayout.PropertyField(sp_DownDelay);
            EditorGUILayout.PropertyField(sp_ChoppedDelay);

            EditorGUILayout.ObjectField(sp_KnifeObjectPrefab);
        }

        serializedObject.ApplyModifiedProperties();
    }

    bool SerializedArrayContains(SerializedProperty property, string propertyRelative, Object obj)
    {
        if (!property.isArray)
            return false;

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty prop = property.GetArrayElementAtIndex(i).FindPropertyRelative(propertyRelative);

            if (prop != null && prop.objectReferenceValue != null)
            {
                if (prop.objectReferenceValue.Equals(obj))
                    return true;
            }
        }

        return false;
    }
}
