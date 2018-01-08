using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatePath))]
public class PlatePathEditor : Editor {

	void OnSceneGUI() {
		PlatePath script = (PlatePath)target;

		for (int i = 0; i < script.localPathPoints.Length; i++) {
			EditorGUI.BeginChangeCheck ();
			Vector3 pos = Handles.DoPositionHandle (script.transform.position + script.localPathPoints [i], Quaternion.identity);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (script, "Moved point");
				EditorUtility.SetDirty (script);
				script.localPathPoints [i] = pos - script.transform.position;
			}
		}
	}
}
