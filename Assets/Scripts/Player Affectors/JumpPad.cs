using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour {

    public float jumpHeight = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2 controller = other.GetComponent<PlayerController2>();

            if (controller) controller.Jump(jumpHeight);
        }
    }
}
