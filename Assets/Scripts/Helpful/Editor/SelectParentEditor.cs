using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SelectParent))]
public class SelectParentEditor : Editor
{
	GameObject selected;

	public void OnSceneGUI()
	{
		selected = Selection.activeGameObject;
		if (selected.transform.parent != null && selected.gameObject.GetComponent<SelectParent>() != null)
		{
			Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
			selected = null;
		}
	}
}
