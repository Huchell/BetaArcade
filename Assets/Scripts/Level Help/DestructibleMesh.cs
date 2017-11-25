using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class DestructibleMesh : MonoBehaviour {

    public Destructible swapObject;

    private new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    public void DestructObject(float amount, Vector3 position, float radius)
    {
        Destructible newObject = Instantiate(swapObject);
        newObject.transform.position = transform.position;
        //newObject.transform.rotation = transform.rotation;
        newObject.transform.localScale = transform.localScale;

        rigidbody.AddExplosionForce(amount, position, radius);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DestructObject(collision.impulse.magnitude * 100, collision.contacts[0].point, 10);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DestructibleMesh))]
public class DestructibleMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Destruct")) { (target as DestructibleMesh).DestructObject(10, (target as DestructibleMesh).transform.position, 10); }
    }
}
#endif
