using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KnockOff : MonoBehaviour {

    public Rigidbody objectToPushOff;


    [SerializeField] private Vector3 force;
    private Vector3 m_objectDestination;

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

    private bool knocked;
    private void Start()
    {
        if (!objectToPushOff) objectToPushOff = GetComponent<Rigidbody>();
        objectToPushOff.Sleep();
    }
    public void PushObject()
    {
        if (!knocked)
        {
            objectToPushOff.AddForce(force, ForceMode.Impulse);
            knocked = true;
        }
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
