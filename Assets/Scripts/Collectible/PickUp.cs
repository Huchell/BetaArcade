using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour {

    private void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            
            other.gameObject.GetComponent<PlayerController2>().OnCollectCollectable(); //usedForSound
            PlayerManager.Instance.onButtonCollect(value);
        }
    }
}


