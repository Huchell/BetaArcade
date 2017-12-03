using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour {

    [Header("World Interaction")]
    [SerializeField]
    [Tooltip("The magnitude of force that need to be exceeded to destroy this object. (Default=5)")]
    private float DestroyThreshold = 5f;

    [Header("Destructible Mesh")]
    [SerializeField]
    [Tooltip("The Prefab of the destructible that will spawn")]
    private GameObject DestructibleMesh;
    [SerializeField]
    [Tooltip("The offset postion from the gameObject that the destructible object will spawn (Default=(0,0,0))")]
    private Vector3 positionOffset;
    [SerializeField]
    [Tooltip("The offset rotation from the gameObject that the destructible object will spawn (Default=(0,0,0)")]
    private Vector3 eulerRotationOffset;

    private bool initialized = false;

    IEnumerator Start() { yield return null; initialized = true; }
    private void OnChargeHit(CatController controller)
    {
        controller.IgnoreHit();
        DestroyMesh();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (initialized)
        {
            if (collision.impulse.magnitude > DestroyThreshold)
            {
                DestroyMesh();
            }
        }
    }

    public void DestroyMesh()
    {
        GetComponent<BoxCollider>().enabled = false;

        GameObject destructibleObject = Instantiate(DestructibleMesh, transform.position + positionOffset, transform.rotation, transform.parent);
        destructibleObject.transform.eulerAngles += eulerRotationOffset;
        destructibleObject.transform.localScale = transform.localScale;

        Destroy(gameObject);
    }
}
