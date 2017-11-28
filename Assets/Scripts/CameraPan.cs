using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {
    public Transform[] target;
    public float speed;
    private int current;

	void FixedUpdate ()
    {
		if (transform.position != target[current].position)
        {
            Vector3 pos2 = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(pos2);
        }
        else
        {
            current = (current + 1) % target.Length;
        }
    }
}
