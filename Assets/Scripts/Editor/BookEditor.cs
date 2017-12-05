using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BooksComeAndGoLogic))]
public class BookEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BooksComeAndGoLogic script = (BooksComeAndGoLogic)target;
        if (GUILayout.Button("Update Books"))
        {
            Undo.RecordObject(script, "UpdateBooks");
            script.UpdateBooks();
        }
        if (GUILayout.Button ("Reset Books"))
        {
            Undo.RecordObject(script, "ResetBooks");
            script.ResetBooks();
		}
    }
}
