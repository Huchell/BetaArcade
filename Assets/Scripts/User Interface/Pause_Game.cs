using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_Game : MonoBehaviour {

    public Transform canvas;
	
	void Start ()
    {
		


	}
	
	
	void Update ()
    {
		
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (canvas.gameObject.activeInHierarchy == false)
            {
                canvas.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                canvas.gameObject.SetActive(false);
                Time.timeScale = 1;
            }

        }

	}
}
