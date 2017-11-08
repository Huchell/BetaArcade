using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rollingPinLoop : MonoBehaviour {

	Rigidbody rb;
	bool started = false;
	Vector3 startLocation;
	Quaternion startRotation;
	public GameObject hitTarget, endTarget;

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();	
		startLocation = transform.position;
		startRotation = transform.rotation;
		InitialImpulse ();
	}
		
	public void InitialImpulse()
	{
		rb.velocity = Vector3.zero;
		rb.rotation = startRotation;
		rb.AddForce (1f, -2f, 0f, ForceMode.VelocityChange);
	}

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject == hitTarget)
		{
			rb.AddForce (2f, 0f, 0f, ForceMode.VelocityChange);
		}

		if (col.gameObject == endTarget)
		{
				Debug.Log ("RESETTING");
				transform.position = startLocation;
				InitialImpulse ();
		}
	}
}
