using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rollingPinStartOnEnter : MonoBehaviour {

    public GameObject rollingPin;
    rollingPinLoop rollingPinLogic;

    private void Start()
    {
        rollingPinLogic = rollingPin.GetComponent<rollingPinLoop>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            rollingPinLogic.loopLoad();
            Destroy(gameObject);
        }        
    }
}
