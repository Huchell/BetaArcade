using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(platform_Oscillate))]
public class platform_Oscillate_Editor : Editor
{
	GameObject solidPlatform;
	Vector3 lerpPosition;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		platform_Oscillate script = (platform_Oscillate)target;
		if (GUILayout.Button ("Refresh Platforms"))
		{
			script.RefreshPlatforms ();
		}
	}

	public void OnSceneGUI()
	{
		platform_Oscillate platform = (platform_Oscillate)target;

		solidPlatform = platform.platform;

		/*
		if (platform.mesh.GetComponent<MeshFilter> ().sharedMesh != null)
		{
			platform.startPosition.GetComponent<MeshFilter> ().mesh = platform.mesh.GetComponent<MeshFilter> ().sharedMesh;
			platform.endPosition.GetComponent<MeshFilter> ().mesh = platform.mesh.GetComponent<MeshFilter> ().sharedMesh;
		}*/

		float lerpX = (1 - platform.offset) * platform.startPosition.transform.position.x + (platform.offset) * platform.endPosition.transform.position.x;
		float lerpY = (1 - platform.offset) * platform.startPosition.transform.position.y + (platform.offset) * platform.endPosition.transform.position.y;
		float lerpZ = (1 - platform.offset) * platform.startPosition.transform.position.z + (platform.offset) * platform.endPosition.transform.position.z;
		lerpPosition = new Vector3 (lerpX, lerpY, lerpZ);

		solidPlatform.transform.SetPositionAndRotation (lerpPosition, Quaternion.identity);

		if (platform.offset < 0.05 && !platform.AlwaysShowGhosts)
		{
			platform.startPosition.GetComponent<MeshRenderer> ().enabled = false;
		}
		else
		{
			platform.startPosition.GetComponent<MeshRenderer> ().enabled = true;
		}

		if (platform.offset > 0.95 && !platform.AlwaysShowGhosts)
		{
			platform.endPosition.GetComponent<MeshRenderer> ().enabled = false;
		}
		else
		{
			platform.endPosition.GetComponent<MeshRenderer> ().enabled = true;
		}

		platform.platform.transform.localScale = platform.selfScale;
		platform.startPosition.transform.localScale = platform.endPosition.transform.localScale = platform.selfScale;
	}
}
