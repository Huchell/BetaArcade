﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class AddForceOnEvent : MonoBehaviour {

    [SerializeField] private bool m_SleepOnStart;
    [SerializeField] private Vector3 m_force;
    [SerializeField] private ForceMode m_ForceMode;

    private IEnumerator Start()
    {
        yield return null;
        if (m_SleepOnStart) GetComponent<Rigidbody>().Sleep();
    }
    public void AddForce()
    {
        GetComponent<Rigidbody>().AddForce(m_force, m_ForceMode);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AddForceOnEvent))]
public class AddForceOnEventEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Add Force"))
        {
            (target as AddForceOnEvent).AddForce();
        }
    }
}
#endif