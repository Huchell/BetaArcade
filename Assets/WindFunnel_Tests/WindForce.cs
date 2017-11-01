using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindForce : MonoBehaviour
{

    List<Rigidbody> RigidbodiesInWindZoneList = new List<Rigidbody>();

    [Tooltip("World direction of the wind")]
    public Vector3 windDirection = Vector3.up;

    [Tooltip("How strong the wind is")]
    public float windStrength = 10;

    private void OnTriggerEnter(Collider col)
    {
        Rigidbody objectRigid = col.gameObject.GetComponent<Rigidbody>();
        if (objectRigid != null)
            RigidbodiesInWindZoneList.Add(objectRigid);
    }

    private void OnTriggerExit(Collider col)
    {
        Rigidbody objectRigid = col.gameObject.GetComponent<Rigidbody>();
        if (objectRigid != null)
            RigidbodiesInWindZoneList.Remove(objectRigid);
    }

    private void FixedUpdate()
    {

        if (RigidbodiesInWindZoneList.Count > 0)
        {
            foreach (Rigidbody rigid in RigidbodiesInWindZoneList)
                rigid.AddForce(windDirection * windStrength);
        }
    }
}

