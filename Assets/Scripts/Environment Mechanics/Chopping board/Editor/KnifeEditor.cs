using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Knife))]
public class KnifeEditor : Editor {

    Knife knife;

    private void OnEnable()
    {
        knife = target as Knife;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);


        if (GUILayout.Button("Chop"))
        {
            knife.ChopFully();
        }

        EditorGUI.EndDisabledGroup();
    }
}
