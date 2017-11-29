using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rollingPinLoop : MonoBehaviour {

	Rigidbody rb;
	bool started = false;
	Vector3 startLocation;
	Quaternion startRotation;
	public GameObject hitTarget, endTarget;
    public Vector3 InitialForce = new Vector3(1f, -2f, 0f), GroundVelocityBoost = new Vector3(2f, 0f, 0f);

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();	
		startLocation = transform.position;
		startRotation = transform.rotation;
		InitialImpulse ();
        hitTarget.GetComponent<MeshRenderer>().enabled = false;
        endTarget.GetComponent<MeshRenderer>().enabled = false;
    }
		
	public void InitialImpulse()
	{
		rb.velocity = Vector3.zero;
		rb.rotation = startRotation;
		rb.AddForce (InitialForce, ForceMode.VelocityChange);
	}
    
	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject == hitTarget)
		{
			rb.AddForce (GroundVelocityBoost, ForceMode.VelocityChange);
		}

		if (col.gameObject == endTarget)
		{
				//Debug.Log ("RESETTING");
				transform.position = startLocation;
				InitialImpulse ();
		}
	}
}
