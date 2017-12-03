using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestructibleMesh : MonoBehaviour {

    [SerializeField]
    [Tooltip("The force at which the shards will explode from the objects origin. (Default=1)")]
    private float explosionRadius = 1f;
    [SerializeField]
    [Tooltip("The life time of the shards, if less than 0 it doesn't destroy itself (Default=5)")]
    private float lifeTime = 5f;

    public float ExplosionRadius
    {
        get
        {
            return explosionRadius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    public IEnumerator Start()
    {
        yield return null;

        Vector3 position = transform.position;

        Collider[] collisions = Physics.OverlapSphere(position, 1f, LayerMask.GetMask("Destructible Shard")).Where(c => c.transform.parent.parent == transform).ToArray();

        foreach (Collider col in collisions)
        {
            if (col.attachedRigidbody)
            {
                col.attachedRigidbody.AddExplosionForce(5f, position, ExplosionRadius, 0, ForceMode.Impulse);
            }
        }

        if (lifeTime >= 0)
            Destroy(gameObject, lifeTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }


}
