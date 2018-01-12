using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IngredientPickup : MonoBehaviour {

    [SerializeField]
    private UnityEvent OnPickup;

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

            other.gameObject.GetComponent<PlayerController2>().OnCollectIngredient();
            OnPickup.Invoke();
        }
    }
}


