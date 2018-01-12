using UnityEngine;
using System.Collections;

public class Buoyancy : MonoBehaviour
{
    public float waterLevel;
    public float floatHeight;
    public float bounceDamp;

    private float forceFactor;
    private Vector3 actionPoint;
    private Vector3 uplift;
    Vector3 buoyancyCentreOffset;

    void Update()
    {
        actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
        forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);

        if (forceFactor > 0f)
        {
            uplift = -Physics.gravity * (forceFactor - GetComponent<Rigidbody>().velocity.y * bounceDamp);
            GetComponent<Rigidbody>().AddForceAtPosition(uplift, actionPoint);
        }
    }
}
