using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DestructibleObject))]
public class DestructibleObjectEditor : Editor {

    DestructibleObject castedTarget;

    private void OnEnable()
    {
        castedTarget = (target as DestructibleObject);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying && GUILayout.Button("Destroy Object"))
        {
            castedTarget.DestroyMesh();
        }
    }
}
