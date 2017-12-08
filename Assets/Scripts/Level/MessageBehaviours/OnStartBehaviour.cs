using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartBehaviour : MonoBehaviour {

    public UnityEngine.Events.UnityEvent OnStart;

	// Use this for initialization
	void Start ()
    {
        OnStart.Invoke();
	}
}
