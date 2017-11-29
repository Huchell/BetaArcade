using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platform_Ring_Spin : MonoBehaviour {

	[Tooltip("Rotations per second. 0.1f means it will take 10 seconds for one full rotation.")]
	public float RPS = 0.1f;
	private MeshFilter mf;
	//private Rigidbody rb;
	//float worldTime;

	// Use this for initialization
	void Start ()
	{
		mf = GetComponent<MeshFilter> ();
		mf.mesh = null;
		//rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.RotateAround (transform.position, Vector3.up, 360f * RPS * Time.deltaTime);

		/*
		worldTime += Time.deltaTime;
		if (worldTime > (1 / RPS)) {
			worldTime -= (1 / RPS);
		}

		rb.MoveRotation(Quaternion.AngleAxis(360f * RPS * worldTime, Vector3.up));
		*/
	}
}
