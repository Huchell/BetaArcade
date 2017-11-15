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
        if (GUILayout.Button("Reset All Node Positions"))
        {
            script.RefreshNodeLocations();
        }
    }

    public void OnSceneGUI()
    {
        BoxPush script = (BoxPush)target;

        for (int node = 0; node < script.nodeList.Count; node++)
        {
            script.nodeList[node] = script.nodesInWorld[node].transform.position;
        }

    }
}
