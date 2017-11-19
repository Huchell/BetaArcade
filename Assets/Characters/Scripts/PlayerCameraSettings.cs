using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSettings : MonoBehaviour {

    [SerializeField]
    private ThirdPersonCamera m_CameraReference;
    public ThirdPersonCamera CameraReference
    {
        get
        {
            return m_CameraReference;
        }
        set
        {
            m_CameraReference = value;
        }
    }

    [SerializeField]
    private Vector2 m_LookSensitivity = Vector2.one;
    public Vector2 LookSensitivity
    {
        get
        {
            return m_LookSensitivity;
        }
        set
        {
            LookSensitivityX = value.x;
            LookSensitivityY = value.y;
        }
    }
    public float LookSensitivityX
    {
        get
        {
            return m_LookSensitivity.x;
        }
        set
        {
            value = Mathf.Max(0.1f, value);
            m_LookSensitivity.x = value;
        }
    }
    public float LookSensitivityY
    {
        get
        {
            return m_LookSensitivity.y;
        }
        set
        {
            value = Mathf.Max(0.1f, value);
            m_LookSensitivity.y = value;
        }
    }


}
