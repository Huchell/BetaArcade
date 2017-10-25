using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlingPlayerTo : MonoBehaviour {

    Vector2 xzDirection, xzVelocity; float xzDistance, yDistance, yVelocity, flightTime = 1.5f, gravity = -9.81f;
    Vector3 finalForce;

    public GameObject target;

    private void OnCollisionEnter(Collision player)
    {
        if (player.transform.tag == "Player")
        {
            Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();

            xzDirection = new Vector2(target.transform.position.x - player.transform.position.x, target.transform.position.z - player.transform.position.z);
            xzDirection.Normalize();
            xzDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(target.transform.position.x, target.transform.position.z));

            xzVelocity = (xzDistance / flightTime)*xzDirection;
            yVelocity = (target.transform.position.y - player.transform.position.y) - (0.5f * gravity * flightTime);

            finalForce = new Vector3(xzVelocity.x, yVelocity, xzVelocity.y);

            rb.AddForce(finalForce, ForceMode.VelocityChange);
        }
    }

}
