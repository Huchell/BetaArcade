using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoxPush))]
public class BoxPushEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BoxPush script = (BoxPush)target;
        if (GUILayout.Button("Refresh Nodes"))
        {
            script.RefreshNodes();
        }
    }
}
