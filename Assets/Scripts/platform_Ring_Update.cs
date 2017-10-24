using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class platform_Ring_Update : MonoBehaviour {

	public int numberOfPlatforms;
	private GameObject[] platforms;
	public float radius = 5f;
	public GameObject platformType;
	public Vector3 platformScale = new Vector3(1.0f,1.0f,1.0f);

	private bool update = true;

	public void OnValidate()
	{
		if (numberOfPlatforms > 32)
		{
			numberOfPlatforms = 32;
		}

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
}
