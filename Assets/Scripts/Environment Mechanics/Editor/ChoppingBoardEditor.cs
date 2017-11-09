using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChoppingBoard))]
public class ChoppingBoardEditor : Editor {

    SerializedProperty
        sp_Knives,
        sp_DefaultKnife,
        sp_KnifeObjectPrefab;

    bool showSettings;
    Transform knifeHolder;
    ChoppingBoard cp_target;

    private void OnEnable()
    {
        sp_Knives = serializedObject.FindProperty("knives");
        sp_DefaultKnife = serializedObject.FindProperty("defaultKnife");
        sp_KnifeObjectPrefab = serializedObject.FindProperty("knifeObjectPrefab");

        cp_target = target as ChoppingBoard;

        knifeHolder = cp_target.transform.Find("knifeHolder");

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
            sp_Knives.arraySize++;
            SerializedProperty newKnife = sp_Knives.GetArrayElementAtIndex(sp_Knives.arraySize - 1);
            GameObject newKnifeObject;

            if (sp_KnifeObjectPrefab.objectReferenceValue == null)
            {
                newKnifeObject = new GameObject("Knife_" + (sp_Knives.arraySize - 1));
                new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer)).transform.SetParent(newKnifeObject.transform);
                newKnifeObject.transform.SetParent(knifeHolder);
            }
            else
            {
                newKnifeObject = Instantiate(sp_KnifeObjectPrefab.objectReferenceValue) as GameObject;
                newKnifeObject.transform.SetParent(knifeHolder);
            }

            newKnife.FindPropertyRelative("knifeRoot").objectReferenceValue = newKnifeObject;
            newKnife.FindPropertyRelative("path").animationCurveValue = sp_DefaultKnife.FindPropertyRelative("path").animationCurveValue;
            newKnife.FindPropertyRelative("speed").floatValue = sp_DefaultKnife.FindPropertyRelative("speed").floatValue;
            newKnife.FindPropertyRelative("move").boolValue = sp_DefaultKnife.FindPropertyRelative("move").boolValue;
            newKnife.FindPropertyRelative("delay").floatValue = sp_DefaultKnife.FindPropertyRelative("delay").floatValue;
        }

        GUI.enabled = sp_Knives.arraySize > 0;

        if (GUILayout.Button(new GUIContent("-")))
        {
            sp_Knives.DeleteArrayElementAtIndex(sp_Knives.arraySize - 1);
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

                SerializedProperty prop_knifeRoot = prop.FindPropertyRelative("knifeRoot");
                SerializedProperty prop_Path = prop.FindPropertyRelative("path");
                SerializedProperty prop_Speed = prop.FindPropertyRelative("speed");
                SerializedProperty prop_delay = prop.FindPropertyRelative("delay");
                SerializedProperty prop_Move = prop.FindPropertyRelative("move");

                #region Root

                EditorGUI.BeginChangeCheck();

                Object knifeRoot = EditorGUILayout.ObjectField("Root", prop_knifeRoot.objectReferenceValue, typeof(Transform), true);

                if (EditorGUI.EndChangeCheck())
                {
                    if (!SerializedArrayContains(sp_Knives, "knifeRoot", knifeRoot))
                    {
                        prop_knifeRoot.objectReferenceValue = knifeRoot;
                    }
                }

                EditorGUI.BeginDisabledGroup(prop_knifeRoot.objectReferenceValue == null);

                #endregion

                #region Path

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(prop_Path);

                if (EditorGUI.EndChangeCheck())
                {
                    Transform trans = prop_knifeRoot.objectReferenceValue as Transform;
                    trans.position = new Vector3(
                        trans.position.x,
                        prop_Path.animationCurveValue.Evaluate(0),
                        trans.position.z
                        );
                }

                #endregion

                #region Speed

                EditorGUILayout.DelayedFloatField(prop_Speed);

                #endregion

                #region Delay

                EditorGUILayout.DelayedFloatField(prop_delay);

                #endregion  

                #region Move

                prop_Move.boolValue = EditorGUILayout.ToggleLeft(prop_Move.displayName, prop_Move.boolValue);

                #endregion

                EditorGUI.EndDisabledGroup();

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
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Default Values"), EditorStyles.boldLabel);

            SerializedProperty
                dk_path = sp_DefaultKnife.FindPropertyRelative("path"),
                dk_speed = sp_DefaultKnife.FindPropertyRelative("speed"),
                dk_move = sp_DefaultKnife.FindPropertyRelative("move"),
                dk_delay = sp_DefaultKnife.FindPropertyRelative("delay");

            EditorGUILayout.PropertyField(dk_path);
            EditorGUILayout.PropertyField(dk_speed);
            EditorGUILayout.PropertyField(dk_move);
            EditorGUILayout.PropertyField(dk_delay);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

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
