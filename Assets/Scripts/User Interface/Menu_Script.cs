using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu_Script : MonoBehaviour {

    public Button Button;


    void Start ()
    {

        Button btn = Button.GetComponent<Button>();
        //btn.onClick.AddListener(OnClick);

    }


    public void OnClick ()
    {

        Debug.Log("To Level");
        SceneManager.LoadScene("Level_Whitebox_LD");
       
	}

    public void OnClickQuit()
    {

        Debug.Log("To Level");
        SceneManager.LoadScene("UI_Test");

    }

    public void CloseGame()
    {

        Application.Quit();
        Debug.Log("Quit");

    }
}
