using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChoppingBoard))]
public class ChoppingBoardEditor : Editor {

    SerializedProperty
        sp_Knives,
        sp_ChopOnStart,
        sp_UpDelay, sp_HoldDelay, sp_DownDelay, sp_ChoppedDelay, sp_WaitForKnives,
        sp_KnifeObjectPrefab;

    bool showSettings;
    ChoppingBoard cp_target;

    private void OnEnable()
    {
        GetProperties();

        cp_target = target as ChoppingBoard;
    }
    void GetProperties()
    {
        sp_Knives = serializedObject.FindProperty("knives");
        sp_ChopOnStart = serializedObject.FindProperty("m_ChopOnStart");

        sp_UpDelay = serializedObject.FindProperty("m_upDelay");
        sp_HoldDelay = serializedObject.FindProperty("m_holdDelay");
        sp_DownDelay = serializedObject.FindProperty("m_downDelay");
        sp_ChoppedDelay = serializedObject.FindProperty("m_choppedDelay");
        sp_WaitForKnives = serializedObject.FindProperty("m_WaitForKnives");

        sp_KnifeObjectPrefab = serializedObject.FindProperty("knifeObjectPrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TopBarGUI();

        KnivesGUI();

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

            EditorGUILayout.PropertyField(sp_WaitForKnives);

            EditorGUILayout.ObjectField(sp_KnifeObjectPrefab);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void TopBarGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(sp_Knives.displayName);

        EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth / 6));

        if (GUILayout.Button(new GUIContent("+"))) AddKnife();

        GUI.enabled = sp_Knives.arraySize > 0;

        if (GUILayout.Button(new GUIContent("-"))) RemoveKnife(sp_Knives.arraySize - 1);

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
    }

    void KnivesGUI()
    {
        for (int i = 0; i < sp_Knives.arraySize; i++)
        {
            SerializedProperty prop = sp_Knives.GetArrayElementAtIndex(i);

            DrawKnife(prop, i);
        }
    }

    void DrawKnife(SerializedProperty knife, int index)
    {
        GUIStyle foldoutStyle = EditorStyles.foldout, buttonStyle = GUI.skin.button;

        Rect barRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
        float deleteWidth = (barRect.width / 6);

        Rect
            foldRect = new Rect(barRect.x, barRect.y, foldoutStyle.margin.left, barRect.height),
            deleteRect = new Rect(barRect.xMax - deleteWidth, barRect.y, deleteWidth, barRect.height),
            objectRect = new Rect(barRect.x + foldRect.width, barRect.y, barRect.width / 2, barRect.height);

        knife.isExpanded = EditorGUI.Foldout(foldRect, knife.isExpanded, GUIContent.none);

        knife.objectReferenceValue = EditorGUI.ObjectField(objectRect, knife.objectReferenceValue, typeof(Knife), true);

        if (GUI.Button(deleteRect, "-"))
        {
            RemoveKnife(index);
            return;
        }

        if (knife.isExpanded)
        {
            Knife knifeObject = knife.objectReferenceValue as Knife;

            EditorGUI.indentLevel++;

            knifeObject.transform.localPosition = EditorGUILayout.Vector3Field("Position", knifeObject.transform.localPosition);
            knifeObject.transform.localEulerAngles = EditorGUILayout.Vector3Field("Rotation", knifeObject.transform.localEulerAngles);
            knifeObject.transform.localScale = EditorGUILayout.Vector3Field("Scale", knifeObject.transform.localScale);

            EditorGUI.indentLevel--;
        }
    }

    void AddKnife()
    {
        cp_target.AddKnife();
        Undo.RecordObject(cp_target, "Added Knife");
        serializedObject.Update();
    }
    void RemoveKnife(int index)
    {
        cp_target.RemoveKnife(index);
        Undo.RecordObject(cp_target, "Removed Knife");
        serializedObject.Update();
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

    private void OnSceneGUI()
    {
        for (int i = 0; i < sp_Knives.arraySize; i++)
        {
            SerializedProperty knifeProp = sp_Knives.GetArrayElementAtIndex(i);

            if (knifeProp.objectReferenceValue != null)
            {
                if (knifeProp.isExpanded)
                {
                    KnifeTransform(cp_target.knives[i]);
                }
            }
        }
    }

    void KnifeTransform(Knife knife)
    {
        if (knife.isActiveAndEnabled)
        {
            switch (Tools.current)
            {
                
                case Tool.Move: KnifePosition(knife); break;
                case Tool.Rotate: KnifeRotation(knife); break;
                case Tool.Scale: KnifeScale(knife); break;

                case Tool.View: 
                case Tool.Rect: 
                case Tool.None: break;
            }
        }
    }

    void KnifePosition(Knife knife)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 knifePos = Handles.PositionHandle(knife.transform.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move Knife");
            knife.transform.position = knifePos;
            Repaint();
        }
    }
    void KnifeRotation(Knife knife)
    {
        EditorGUI.BeginChangeCheck();

        Quaternion knifeRot = Handles.RotationHandle(knife.transform.rotation, knife.transform.position);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Rotate Knife");
            knife.transform.rotation = knifeRot;
            Repaint();
        }
    }
    void KnifeScale(Knife knife)
    {
        EditorGUI.BeginChangeCheck();

        Vector3 scale = Handles.ScaleHandle(knife.transform.localScale, knife.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(knife.transform.position));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Scale Knife");
            knife.transform.localScale = scale;
            Repaint();
        }
    }
}
