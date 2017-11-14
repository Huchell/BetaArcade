using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour {

    public int damage;
    PlayerController pc;
    Rigidbody rb;
    public float bounceback = 15f;

    private void OnCollisionEnter(Collision collision)
    {
        try
        {
            if (collision.gameObject.GetComponent<PlayerController>() != null)
            {
                pc = collision.gameObject.GetComponent<PlayerController>();
                rb = collision.gameObject.GetComponent<Rigidbody>();
                pc.OnDamage(damage);
                Vector3 direction = transform.position - collision.gameObject.transform.position;
                direction.Normalize();
                rb.AddForce((Vector3.up * 0.75f * bounceback)+new Vector3(direction.x*-bounceback, 0, direction.z*-bounceback), ForceMode.Impulse);
            }
        }
        catch
        {
            Debug.Log("Trying to get controller when it doesn't exist throws an error. Whoops.");
        }
    }
}
