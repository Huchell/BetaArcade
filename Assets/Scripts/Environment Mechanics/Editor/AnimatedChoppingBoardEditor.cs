using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimatedChoppingBoard))]
public class AnimatedChoppingBoardEditor : Editor {

    SerializedProperty sp_Knives, sp_KnifePrefab;

    GUILayoutOption smallButtonHeightOption = GUILayout.Height(EditorGUIUtility.singleLineHeight);

    private void OnEnable()
    {
        sp_Knives = serializedObject.FindProperty("Knives");
        sp_KnifePrefab = serializedObject.FindProperty("knifePrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sp_KnifePrefab.objectReferenceValue = EditorGUILayout.ObjectField("Knife Prefab", sp_KnifePrefab.objectReferenceValue, typeof(Knife), true);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        ArrayHeader();

        ArrayBody();

        serializedObject.ApplyModifiedProperties();
    }

    void ArrayHeader()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(sp_Knives.displayName, EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth / 6));

        if (GUILayout.Button("+", smallButtonHeightOption))
        {
            int index = sp_Knives.arraySize;
            sp_Knives.InsertArrayElementAtIndex(index);
            SerializedProperty newProp = sp_Knives.GetArrayElementAtIndex(index);

            if (sp_KnifePrefab.objectReferenceValue != null)
            {
                newProp.objectReferenceValue = Instantiate(sp_KnifePrefab.objectReferenceValue);
            }

            Transform trans = (newProp.objectReferenceValue as Knife).transform;
            trans.SetParent((target as AnimatedChoppingBoard).transform, false);
            trans.localPosition = Vector3.zero;
        }

        EditorGUI.BeginDisabledGroup(sp_Knives.arraySize == 0);

        if (GUILayout.Button("-", smallButtonHeightOption))
        {
            (target as AnimatedChoppingBoard).RemoveKnife(sp_Knives.arraySize - 1);
            serializedObject.Update();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
    }

    void ArrayBody()
    {
        if (sp_Knives.arraySize > 0)
        {
            for (int i = 0; i < sp_Knives.arraySize; i++)
            {
                SerializedProperty arrayProp = sp_Knives.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = false;

                arrayProp.objectReferenceValue = EditorGUILayout.ObjectField(
                    arrayProp.objectReferenceValue.name,
                    arrayProp.objectReferenceValue,
                    typeof(Knife),
                    true
                    );

                GUI.enabled = true;

                /*if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.currentViewWidth / 12), smallButtonHeightOption))
                {
                    (target as AnimatedChoppingBoard).RemoveKnife(i);
                }*/

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Knives have been created", MessageType.Info);
        }

        
    }
}
