using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour {

    [Header("World Interaction")]
    [SerializeField]
    [Tooltip("The magnitude of force that need to be exceeded to destroy this object. (Default=5)")]
    private float DestroyThreshold = 5f;
    [SerializeField]
    [Tooltip("The layers that wont count towards destroying the mesh.")]
    private LayerMask ignoreLayers = 0;

    [Header("Destructible Mesh")]
    [SerializeField]
    [Tooltip("The Destructible that will apear and shatter")]
    private GameObject DestructibleMesh;
    [SerializeField]
    [Tooltip("")]
    private GameObject NormalMesh;
    [SerializeField]
    [Tooltip("Seconds that the destructible will stay in the scene after it is destroyed")]
    private float Lifetime = 2f;
    [Space]
    public UnityEngine.Events.UnityEvent OnBroken;

    private bool initialized = false;

    private void Awake()
    {
        DestructibleMesh.SetActive(false);
        NormalMesh.SetActive(true);
    }

    IEnumerator Start()
    {
        yield return null;
        initialized = true;
    }

    private void OnChargeHit(CatController controller)
    {
        if (initialized)
        {
            controller.IgnoreHit();
            DestroyMesh();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (initialized && 
            !collision.gameObject.CompareTag("Player") && 
            collision.relativeVelocity.magnitude > DestroyThreshold)
        {
            DestroyMesh();
        }
    }

    public void DestroyMesh()
    {
        GetComponent<MeshCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        NormalMesh.SetActive(false);
        DestructibleMesh.SetActive(true);

        OnBroken.Invoke();
        Destroy(gameObject, Lifetime);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (DestructibleMesh)
            {
                if (child &&
                    child != DestructibleMesh.transform &&
                    child != NormalMesh.transform)
                    transform.GetChild(i).SetParent(transform.parent, true);
            }
        }
    }
}
