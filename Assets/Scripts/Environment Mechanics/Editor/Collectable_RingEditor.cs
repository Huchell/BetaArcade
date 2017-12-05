using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Collectable_Ring))]
public class Collectable_RingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Collectable_Ring script = (Collectable_Ring)target;
        if (GUILayout.Button("Refresh Collectables"))
        {
            script.RefreshButtons();
        }
    }
}