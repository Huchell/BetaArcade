using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KnockOff : MonoBehaviour {

    public Rigidbody objectToPushOff;

    [SerializeField] private Vector3 m_objectDestination;

    public Vector3 WorldObjectDestination
    {
        get
        {
            return m_objectDestination + transform.position;
        }
        set
        {
            m_objectDestination = value - transform.position;
        }
    }
    public Vector3 LocalObjectDestination
    {
        get
        {
            return m_objectDestination;
        }
        set
        {
            m_objectDestination = value;
        }
    }

	void OnChargeHit(ControllerColliderHit hit)
    {
        objectToPushOff.AddForce(new Vector3(.1f, .1f), ForceMode.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (objectToPushOff)
        {
            MeshFilter mesh = objectToPushOff.GetComponent<MeshFilter>();
            if (mesh)
                Gizmos.DrawMesh(
                    mesh.sharedMesh, 
                    WorldObjectDestination, 
                    objectToPushOff.transform.rotation, 
                    objectToPushOff.transform.localScale
                    );
            else
                Gizmos.DrawSphere(WorldObjectDestination, 0.1f);
        }
        else
            Gizmos.DrawSphere(WorldObjectDestination, 0.1f);
    }
}
