using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Collectable_LineEnd))]
public class Collectable_LineEndEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Refresh Buttons"))
        {
            Collectable_LineEnd script = (Collectable_LineEnd)target;
            Selection.activeGameObject = script.startButton.transform.gameObject;
        }
    }
}
