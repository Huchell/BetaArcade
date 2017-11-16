using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    public bool lockCursor;
    public float mouseSensitivityX = 7;
    public float mouseSensitivityY = 5;
    public Transform target;
    public PlayerController2 player;
    public float distanceFromTraget = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    void Start()
    {
        if (player)
            player.camera = this;
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

        transform.position = target.position - transform.forward * distanceFromTraget;
	}
}
