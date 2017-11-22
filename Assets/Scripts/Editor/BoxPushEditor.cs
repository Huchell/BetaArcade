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
            Undo.RecordObject(script, "RefreshNodes");
            script.RefreshNodes();
        }
        if (GUILayout.Button("Reset All Node Positions"))
        {
            Undo.RecordObject(script, "ResetNodePositions");
            script.RefreshNodeLocations();
        }
        if (GUILayout.Button("Suggest Push Alignments"))
        {
            Undo.RecordObject(script, "SuggestPushAlignments");
            script.SuggestPushAlignments();
        }
    }

    public void OnSceneGUI()
    {
        BoxPush script = (BoxPush)target;

        for (int node = 0; node < script.nodeList.Count; node++)
        {
            Vector3 pos = script.nodesInWorld[node].transform.position;

            EditorGUI.BeginChangeCheck();
            
            pos = Handles.PositionHandle(pos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                script.nodesInWorld[node].transform.position = pos;
                script.nodeList[node] = pos;
            }
            //script.nodeList[node] = script.nodesInWorld[node].transform.position;
        }

    }
}
