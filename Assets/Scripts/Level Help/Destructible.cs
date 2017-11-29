using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Destructible : MonoBehaviour {

    public UnityEngine.Events.UnityEvent OnDestruct;
    
    [Range(0, 30f)]
    public float m_DespawnTimer = 5.0f;

    private void Start()
    {
        OnDestruct.Invoke();
        Destroy(gameObject, m_DespawnTimer);
    }
}
