using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class CatChargeHitbox : MonoBehaviour
{
    private CatController controller;

    private void Start()
    {
        controller = transform.parent.GetComponent<CatController>();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.SendMessageUpwards("OnChargeHit", controller, SendMessageOptions.DontRequireReceiver);
    }
}
