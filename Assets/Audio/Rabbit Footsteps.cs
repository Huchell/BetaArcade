using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitFootsteps : MonoBehaviour {

	public AudioClip footSteps;
	private AudioSource source;
	private float volLowRange = .5f;
	private float volHighRange = 1.0f;

	// Use this for initialization
	void Awake () {

		source = GetComponent<AudioSource> ();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
