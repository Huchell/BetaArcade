using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Scripts/ConstructionScript")]
public class Construction : MonoBehaviour {

    [SerializeField]
    private bool
        checkPosition = true,
        checkRotation = true,
        checkScale = true,
        runConstruction = true,
        sendUpwards = false;

    private Vector3 prev_position;
    private Quaternion prev_rotation;
    private Vector3 prev_scale;

    private bool isDirty
    {
        get
        {
            return
                (checkPosition && prev_position != transform.position) ||
                (checkRotation && prev_rotation != transform.rotation) ||
                (checkScale && prev_scale != transform.localScale);
        }
    }

    private void Update()
    {
        // Check to see if they want to run construction
        if (!runConstruction)
            return;

        ConstructButton();
    }

    void SetRunInEditor(bool value)
    {
        SetRunInEditor(gameObject, value);
    }
    void SetRunInEditor(GameObject obj, bool value)
    {
        foreach (MonoBehaviour mono in obj.GetComponents<MonoBehaviour>())
        {
            mono.runInEditMode = value;
        }
    }
    void SetRunInEditorUpwards(bool value)
    {
        SetRunInEditorUpwards(gameObject, value);
    }
    void SetRunInEditorUpwards(GameObject obj, bool value)
    {
        SetRunInEditor(obj, value);

        if (obj.transform.parent != null)
        {
            SetRunInEditorUpwards(obj.transform.parent.gameObject, value);
        }
    }

    void SendConstructionMessage()
    {
        SetRunInEditor(true);

        SendMessage("OnConstruction", SendMessageOptions.DontRequireReceiver);

        SetRunInEditor(false);
    }
    void SendConstructionMessageUpwards()
    {
        SetRunInEditorUpwards(true);

        SendMessageUpwards("OnConstruction", SendMessageOptions.DontRequireReceiver);

        SetRunInEditorUpwards(false);
    }

    private void Awake()
    {
        if (Application.isPlaying)
            Destroy(this);

        // Check to see if it is dirty
        if (isDirty)
        {
            ConstructButton();
        }

        // Save previous positon, rotation, and scale
        prev_position = transform.position;
        prev_rotation = transform.rotation;
        prev_scale = transform.localScale;
    }

    public void ConstructButton()
    {
        if (sendUpwards)
        {
            SendConstructionMessageUpwards();
        }
        else
        {
            SendConstructionMessage();
        }

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Construction))]
public class ConstructionEditor : Editor
{
    SerializedProperty
        sp_checkPositon,
        sp_checkRotation,
        sp_checkScale,
        sp_runConstruction,
        sp_sendUpwards;

    private void OnEnable()
    {
        sp_checkPositon = serializedObject.FindProperty("checkPosition");
        sp_checkRotation = serializedObject.FindProperty("checkRotation");
        sp_checkScale = serializedObject.FindProperty("checkScale");
        sp_runConstruction = serializedObject.FindProperty("runConstruction");
        sp_sendUpwards = serializedObject.FindProperty("sendUpwards");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sp_runConstruction);
        EditorGUILayout.PropertyField(sp_sendUpwards);

        sp_checkPositon.isExpanded = EditorGUILayout.Foldout(sp_checkPositon.isExpanded, new GUIContent("Check Flags"));

        if (sp_checkPositon.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(sp_checkPositon);
            EditorGUILayout.PropertyField(sp_checkRotation);
            EditorGUILayout.PropertyField(sp_checkScale);

            EditorGUI.indentLevel--;
        }

        if (!sp_runConstruction.boolValue)
            if (GUILayout.Button("Construct"))
            {
                (target as Construction).ConstructButton();
            }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
