using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Collectable_Line))]
public class Collectable_LineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Collectable_Line script = (Collectable_Line)target;
        if (GUILayout.Button("Refresh Collectables"))
        {
            script.RefreshButtons();
        }
    }

    public void OnSceneGUI()
    {
        Collectable_Line script = (Collectable_Line)target;
        script.RefreshButtons();
    }
}