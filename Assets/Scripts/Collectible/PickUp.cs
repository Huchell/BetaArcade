using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour {

    private void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Do things
            Debug.Log("Pick Up");
            gameObject.SetActive(false);

            //other.gameObject.SetActive(false);
            //count = count + 1;
            //SetCountText();
        }
    }
}


