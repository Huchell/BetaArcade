using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

public class PlayerCameraSettings : MonoBehaviour {

    [SerializeField] private ThirdPersonCamera m_CameraReference;
    [SerializeField] private Vector2 m_LookSensitivity = Vector2.one;
    [Range(0, 10)][SerializeField] private float m_CameraDistance = 1;
    [SerializeField] private Vector2 m_PitchMinMax = new Vector2(-40, 85);
    [SerializeField] private float m_rotationSmoothTime = 0.12f;

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

    private void Start()
    {
        if (CameraReference)
        {
            CameraReference.player = GetComponent<PlayerController2>();
            CameraReference.mouseSensitivityX = LookSensitivityX;
            CameraReference.mouseSensitivityY = LookSensitivityY;
            CameraReference.distanceFromTraget = m_CameraDistance;
            CameraReference.pitchMinMax = m_PitchMinMax;
            CameraReference.rotationSmoothTime = m_rotationSmoothTime;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerCameraSettings))]
public class PlayerCameraSettingsEditor : Editor
{
    SerializedProperty
        sp_CameraReference,
        sp_LookSensitivity,
        sp_CameraDistance,
        sp_PitchMinMax,
        sp_RotationSmoothTime;

    AnimBool CameraSettingsDrop;

    private void OnEnable()
    {
        sp_CameraReference      = serializedObject.FindProperty("m_CameraReference");
        sp_LookSensitivity      = serializedObject.FindProperty("m_LookSensitivity");
        sp_CameraDistance       = serializedObject.FindProperty("m_CameraDistance");
        sp_PitchMinMax          = serializedObject.FindProperty("m_PitchMinMax");
        sp_RotationSmoothTime   = serializedObject.FindProperty("m_rotationSmoothTime");

        CameraSettingsDrop = new AnimBool(sp_CameraReference.objectReferenceValue != null);
        CameraSettingsDrop.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(sp_CameraReference);

        if (EditorGUI.EndChangeCheck())
        {
            CameraSettingsDrop.target = sp_CameraReference.objectReferenceValue != null;
        }

        if (EditorGUILayout.BeginFadeGroup(CameraSettingsDrop.faded))
        {
            EditorGUILayout.PropertyField(sp_LookSensitivity);
            EditorGUILayout.PropertyField(sp_CameraDistance);
            EditorGUILayout.PropertyField(sp_PitchMinMax);
            EditorGUILayout.PropertyField(sp_RotationSmoothTime);
        }

        EditorGUILayout.EndFadeGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
