using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rollingPinLoop : MonoBehaviour {

	Rigidbody rb;
	bool started = false;
	Vector3 startLocation;
	Quaternion startRotation;
	public GameObject hitTarget, endTarget;
    public Vector3 initialForce = new Vector3(1f, -2f, 0f), groundVelocityBoost = new Vector3(2f, 0f, 0f);
    public int loops = 0;
    public Collider startOnEnter;
    bool handleLoops = false;

	// Use this for initialization
	void Start ()
	{
		rb = gameObject.GetComponent<Rigidbody>();	
		startLocation = transform.position;
		startRotation = transform.rotation;
        hitTarget.GetComponent<MeshRenderer>().enabled = false;
        endTarget.GetComponent<MeshRenderer>().enabled = false;
        transform.parent.gameObject.GetComponent<MeshRenderer>().enabled = false;

        if (loops > 0)
        {
            handleLoops = true;
            rb.useGravity = false;
        }
        else
        {
            InitialImpulse();
        }
    }

    public void loopLoad()
    {
        try
        {
            rb.useGravity = true;
        }
        catch
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
        }
        loops++;
        InitialImpulse();
    }
		
	public void InitialImpulse()
	{
		rb.velocity = Vector3.zero;
		rb.rotation = startRotation;
		rb.AddForce (initialForce, ForceMode.VelocityChange);
        if (handleLoops)
        {
            loops--;
            if (loops <= 0)
            {
                Destroy(gameObject.transform.parent.gameObject);
            }
        }
    }
    
	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject == hitTarget)
		{
			rb.AddForce (groundVelocityBoost, ForceMode.VelocityChange);
		}

		if (col.gameObject == endTarget)
		{
				//Debug.Log ("RESETTING");
				transform.position = startLocation;
				InitialImpulse ();
		}
	}

    public void ColliderMake()
    {
        startOnEnter = new GameObject("StartOnEnterVolume").AddComponent<BoxCollider>();
        startOnEnter.isTrigger = true;
        startOnEnter.gameObject.transform.SetParent(gameObject.transform.parent);
        startOnEnter.gameObject.transform.localPosition = new Vector3(-2f, 0f, 0f);
        startOnEnter.gameObject.transform.localScale = new Vector3(10f, 10f, 10f);
        startOnEnter.gameObject.AddComponent<rollingPinStartOnEnter>();
        startOnEnter.gameObject.GetComponent<rollingPinStartOnEnter>().rollingPin = gameObject;

    }
    public void ColliderBreak()
    {
        DestroyImmediate(startOnEnter.gameObject);
    }
}
