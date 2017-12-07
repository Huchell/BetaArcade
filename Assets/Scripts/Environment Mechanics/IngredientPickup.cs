using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientPickup : MonoBehaviour {

    private void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Do things
            Destroy(gameObject);

            PlayerController2.ingredientCounter += 1;
            //Debug.Log(other.gameObject.GetComponent<PlayerController2>().ingredientCounter);
        }
    }
}


