using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(platform_Ring_Update))]
public class platform_Ring_PlatformScriptRefresh : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		platform_Ring_Update script = (platform_Ring_Update)target;
		if (GUILayout.Button ("Refresh Platforms"))
		{
			script.RefreshPlatforms ();
		}
	}
}
