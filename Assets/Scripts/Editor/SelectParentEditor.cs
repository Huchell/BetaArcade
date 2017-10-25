using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectParentEditor : Editor {

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		SelectParent p = (target as SelectParent);
		if (Selection.activeGameObject == p.gameObject)
			Selection.activeObject = p.transform.parent.gameObject;
	}
}
