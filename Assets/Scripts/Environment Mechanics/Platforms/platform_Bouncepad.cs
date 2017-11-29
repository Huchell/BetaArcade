using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platform_Bouncepad : MonoBehaviour {

	[Tooltip("Force of bouncepad. 5.0 is equivalent to a standard player jump.")]
	public float forcePower = 15f;

	void OnCollisionEnter (Collision other)
	{
		if (other.gameObject.tag == "Player")
		{
			other.collider.attachedRigidbody.AddForce(transform.TransformDirection(Vector3.up)*forcePower, ForceMode.Impulse);
		}
	}
}
