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
        if (GUILayout.Button ("Refresh Books"))
		{
			script.RefreshBooks();
		}
    }
}
