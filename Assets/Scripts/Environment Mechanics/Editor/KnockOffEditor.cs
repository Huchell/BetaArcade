using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KnockOff))]
public class KnockOffEditor : Editor {

    [SerializeField]
    bool initialize = false;

    KnockOff castedTarget;

    SerializedProperty
        sp_objectTargetLocation;

    private void OnEnable()
    {
        castedTarget = target as KnockOff;
    }

    private void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();

        Vector3 newPosition = Handles.DoPositionHandle(
            castedTarget.WorldObjectDestination, 
            Quaternion.identity
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Object Destination");
            castedTarget.WorldObjectDestination = newPosition;
        }
    }
}
