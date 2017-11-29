using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectParent : MonoBehaviour {

	/*GameObject selected;

	public void OnSceneGUI()
	{
		selected = Selection.activeGameObject;
		if (selected != null)
		switch (selected.transform.parent.tag.ToString())
		{
			case ("PlatformRingCentre"):
			{
				Selection.activeGameObject = selected.transform.parent.gameObject;
				Debug.Log ("FUCKER!");
				selected = null;
				break;
			}
		}
	}*/

	void Awake()
	{
		if (Application.isPlaying)
			Destroy (this);
	}
}
