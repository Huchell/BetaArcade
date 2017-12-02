using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifiedPushBox : MonoBehaviour {

    public Transform Volume;
    public Transform Cube;
    Vector3 initPosition;
    Quaternion initRotation;

    // Use this for initialization
    void Start ()
    {
        initPosition = Cube.position;
        initRotation = Cube.rotation;
	}

    private void ResetCube()
    {
        Debug.Log("woo");
        Cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Cube.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        Cube.transform.position = initPosition;
        Cube.transform.rotation = initRotation;
    }
    
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject == Cube.gameObject)
        {
            ResetCube();
        }
    }
}
