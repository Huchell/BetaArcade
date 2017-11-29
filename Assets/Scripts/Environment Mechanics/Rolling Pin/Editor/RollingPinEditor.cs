using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(rollingPinLoop))]
public class RollingPinEditor : Editor {

    SerializedProperty hitTarget, endTarget, initialForce, groundVelocityBoost, loops, startOnEnter;

    private void OnEnable()
    {
        hitTarget = serializedObject.FindProperty("hitTarget");
        endTarget = serializedObject.FindProperty("endTarget");
        initialForce = serializedObject.FindProperty("initialForce");
        groundVelocityBoost = serializedObject.FindProperty("groundVelocityBoost");
        loops = serializedObject.FindProperty("loops");
        startOnEnter = serializedObject.FindProperty("startOnEnter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(hitTarget);
        EditorGUILayout.PropertyField(endTarget);
        EditorGUILayout.PropertyField(initialForce);
        EditorGUILayout.PropertyField(groundVelocityBoost);

        rollingPinLoop script = (rollingPinLoop)target;

        EditorGUI.BeginChangeCheck();

        int value = EditorGUILayout.IntField(new GUIContent(loops.displayName), loops.intValue);

        if (EditorGUI.EndChangeCheck())
        {
            value = Mathf.Max(value, 0);
            loops.intValue = value;
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        if (value > 0)
        {
            EditorGUILayout.PropertyField(startOnEnter);

            if (!startOnEnter.objectReferenceValue)
            {
                script.ColliderMake();
                serializedObject.Update();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Rolling pin will loop indefinitely.", MessageType.Info);
            
            if (startOnEnter.objectReferenceValue)
            {
                script.ColliderBreak();
                serializedObject.Update();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
