using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBackBehaviour : MonoBehaviour {

    [SerializeField]
    private GameObject teleportPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.transform.position = teleportPosition.transform.position;
        }
    }
}
