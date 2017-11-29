using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platform_Standing_Logic : MonoBehaviour {

	Vector3 selfScale;

	void Start()
	{
		selfScale = transform.lossyScale;
	}

	void OnCollisionEnter (Collision other)
	{
		if (other.collider.tag == "Player")
		{
			other.collider.transform.SetParent (transform);
			//other.collider.transform.localScale.Set (1f / selfScale.x, 1f / selfScale.y, 1f / selfScale.z);
		}
	}

    void OnCollisionExit (Collision other)
	{
		if (other.collider.tag == "Player")
		{
			other.collider.transform.parent = null;
		}
	}
}
