using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StopOnfloor : MonoBehaviour {

    [SerializeField]
    private GameObject floor;

    new private Rigidbody rigidbody { get { return GetComponent<Rigidbody>(); } }

    private void OnCollisionEnter(Collision collision)
    {
        bool stop = false;

        if (floor == null)
            stop = collision.gameObject.tag == "Floor";
        else
            stop = collision.gameObject == floor;

        if (stop)
        {
            rigidbody.Sleep();
            rigidbody.isKinematic = true;
        }
    }
}
