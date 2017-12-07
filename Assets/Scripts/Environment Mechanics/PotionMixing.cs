using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PotionMixing : MonoBehaviour {

    public UnityEvent OnPotionFinished;
    GameObject player;

	// Use this for initialization
	void Start ()
    {
       	
	}

    // Update is called once per frame
    void Update ()
    {
        //If interacted with
        

    }

    private void IngredientCheck(Collider other)
    {
        //if (other.gameObject.GetComponent<PlayerController2>().ingredient1)
        if (PlayerController2.ingredient1)
        {
            Debug.Log("Ingredient 1 Done");
        }

        //if (other.gameObject.GetComponent<PlayerController2>().ingredient2)
        if (PlayerController2.ingredient2)
        {
            Debug.Log("Ingredient 2 Done");
        }

        //if (other.gameObject.GetComponent<PlayerController2>().ingredient3)
        if (PlayerController2.ingredient3)
        {
            Debug.Log("Ingredient 3 Done");
            OnPotionFinished.Invoke();
        }


    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && Input.GetButtonDown(other.gameObject.GetComponent<PlayerController2>().GetInputString("Interaction")))
        {
            IngredientCheck(other);
        }
    }
}
