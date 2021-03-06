﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class platform_Ring_Update : MonoBehaviour {

	[Tooltip("Number of platforms. Capped at 32 for stability; see Adam if this cap is insufficient. You must click 'Refresh Platforms' to make this change.")]
	public int numberOfPlatforms;
	private GameObject[] platforms;
	[Tooltip("Radius of ring of platforms. You must click 'Refresh Platforms' to make this change.")]
	public float radius = 5f;
	[Tooltip("Prefab used for platform and ghosts. You must click 'Refresh Platforms' to make this change.")]
	public GameObject platformType;
	[Tooltip("Platform scale.")]
	public Vector3 platformScale = new Vector3(1.0f,1.0f,1.0f);

	private int platformCountCap = 32;

	private bool update = true;

	public void OnValidate()
	{
		numberOfPlatforms = Mathf.Clamp (numberOfPlatforms, 0, platformCountCap);

		/*
		if (Application.isPlaying)
		{
			update = false;
			return;
		}

		if (platforms != null)
		{
			foreach (GameObject gm in platforms)
			{
				StartCoroutine (DestroyPlatforms(gm));
			}
		}

		if (transform.childCount >= numberOfPlatforms)
		{
			return;
		}

		platforms = new GameObject[numberOfPlatforms];

		for (int count = 0; count < numberOfPlatforms; count++)
		{
			Vector3 offset = Quaternion.AngleAxis ((360f / numberOfPlatforms) * count, Vector3.up) * new Vector3 (0, 0, radius);
			GameObject gm = Instantiate (platformType);
			//GameObject gm = new GameObject ("Sphere" + count);

			//gm.AddComponent<SelectParent> ();
			gm.transform.SetParent (transform, false);
			gm.transform.localPosition += offset;
			gm.transform.RotateAround (gm.transform.position, Vector3.up, (360f / numberOfPlatforms) * count);
			gm.transform.localScale = platformScale;
			gm.transform.parent = transform;

			platforms [count] = gm;
		}*/
	}

	IEnumerator DestroyPlatforms(GameObject platform)
	{
		yield return new WaitForEndOfFrame ();
		DestroyImmediate (platform);
	}

	public void RefreshPlatforms()
	{
		for (int x = transform.childCount-1; x >= 0; x--)
		{
			//StartCoroutine (DestroyPlatforms(transform.GetChild(x).gameObject));
			DestroyImmediate(transform.GetChild(x).gameObject);
		}

		platforms = new GameObject[numberOfPlatforms];

		for (int count = 0; count < numberOfPlatforms; count++)
		{
			Vector3 offset = Quaternion.AngleAxis ((360f / numberOfPlatforms) * count, Vector3.up) * new Vector3 (0, 0, radius);
			GameObject gm = Instantiate (platformType);
			//GameObject gm = new GameObject ("Sphere" + count);

			//gm.AddComponent<SelectParent> ();
			gm.transform.SetParent (transform, false);
			gm.transform.localPosition += offset;
			gm.transform.RotateAround (gm.transform.position, Vector3.up, (360f / numberOfPlatforms) * count);
			gm.transform.localScale = platformScale;
			gm.transform.parent = transform;

			platforms [count] = gm;
		}
	}

	public void AdjustPlatformScale()
	{
		if (platforms != null) {
			foreach (GameObject gm in platforms)
			{
				gm.transform.localScale = platformScale;
			}
		}
	}
}
