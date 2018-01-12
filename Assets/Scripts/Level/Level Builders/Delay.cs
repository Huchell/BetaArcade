using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Delay : MonoBehaviour {

    [SerializeField]
    private float delayTime = 1f;
    [SerializeField]
    private UnityEvent DelayEvent;

    public void TriggerDelay()
    {
        StartCoroutine(DelayCoroutine());
    }

    IEnumerator DelayCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        DelayEvent.Invoke();
    }
}
