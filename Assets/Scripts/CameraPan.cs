using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {
    public Transform[] target;
    public float speed;
    public Transform currentTarget;
    public Transform nextTarget;
    private float lerpBetween = 0;

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

        Vector3 pos = currentTarget.position;

        if (nextTarget != null)
        {
            if (lerpBetween >= 1)
            {
                currentTarget = nextTarget;
                nextTarget = null;
                lerpBetween = 0;
            }
            else
            {
                lerpBetween += Time.deltaTime * 5;
                pos = Vector3.Lerp(pos, nextTarget.position, lerpBetween);
            }
        }
        transform.LookAt(pos);
    }
}
