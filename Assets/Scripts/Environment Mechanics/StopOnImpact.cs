using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StopOnImpact : MonoBehaviour {

    [SerializeField] private GameObject floor;

    new private Rigidbody rigidbody { get { return GetComponent<Rigidbody>(); } }

    private void OnCollisionEnter(Collision collision)
    {
        bool stop = floor ? collision.gameObject == floor : true;

        if (stop)
        {
            rigidbody.Sleep();
            rigidbody.isKinematic = true;
        }
    }
}
