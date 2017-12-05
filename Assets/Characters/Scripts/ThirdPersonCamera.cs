using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    public bool lockCursor;
    [HideInInspector]
    public Transform target;
    public PlayerController2 player;
    public float distanceFromTraget = 2;
    public Vector2 distanceMinMax = new Vector2(.5f, 2);
    public float cameraRadius = -0.5f;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    public LayerMask ignoreLayers;

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    public float mouseSensitivityX;
    public float mouseSensitivityY;

    float yaw;
    float pitch;

    float Distance
    {
        get
        {
            RaycastHit hit;
            if (Physics.SphereCast(target.position, cameraRadius, -transform.forward, out hit, distanceMinMax.y, ~ignoreLayers))
            {
                return Mathf.Max(hit.distance, distanceMinMax.x);
            }

            return distanceMinMax.y;
        }
    }

    void Start()
    {
        if (player)
        {
            player.CameraSettings.CameraReference = this;
            target = player.transform.Find("CameraLookAt");
            if (target == null) target = player.transform;
        }
        else
            enabled = false;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    // Update is called once per frame
    void LateUpdate ()
    {
        if (player.playerNumber <= 0)
            return;

        yaw += Input.GetAxis(player.GetInputString("Mouse X")) * mouseSensitivityX;
        pitch -= Input.GetAxis(player.GetInputString("Mouse Y")) * mouseSensitivityY;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;
        
        transform.position = target.position - transform.forward * Distance;
	}

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, cameraRadius);
    }
}
