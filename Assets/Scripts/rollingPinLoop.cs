using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rollingPinLoop : MonoBehaviour {

	Rigidbody rb;
	bool started = false;
	Vector3 startLocation;
	Quaternion startRotation;
	public GameObject hitTarget, endTarget;
    public float decayTime = 5f;
    float timer;
    public bool delay = false;
    public float delayAmount = 2f;

	// Use this for initialization
	void Start ()
	{
        timer = decayTime;
		rb = GetComponent<Rigidbody> ();
		startLocation = transform.position;
		startRotation = transform.rotation;
        if (!delay)
        {
            InitialImpulse();
        }
        else
        {
            rb.useGravity = false;
            Debug.Log(transform.gameObject);
        }
	}
		
	public void InitialImpulse()
    {
        timer -= decayTime;
        transform.position = startLocation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
		rb.rotation = startRotation;
		rb.AddForce (1f, -4f, 0f, ForceMode.VelocityChange);
	}

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject == hitTarget)
		{
			rb.AddForce (2f, 0f, 0f, ForceMode.VelocityChange);
		}

		if (col.gameObject == endTarget)
		{
				InitialImpulse ();
		}
	}

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > decayTime && !delay)
        {
            InitialImpulse();
        }
        
        if (timer > delayAmount+decayTime && delay)
        {
            delay = false;
            rb.useGravity = true;
            timer -= decayTime+delayAmount;
        }

    }
}
