using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    // Singleton
    private static PlayerManager instance;
    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject pm = new GameObject("Player Manager");
                pm.AddComponent<PlayerManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private PlayerController2[] controllers;

    private void Start()
    {
        controllers = FindObjectsOfType<PlayerController2>();
    }
    private void FixedUpdate()
    {
        if (controllers.Where(c => c.playerNumber > 0).ToArray().Length == 1)
        {
            if (Input.GetButtonDown("Switch"))
            {
                int index = GetCurrentControllerIndex();

                SwitchPlayer(controllers[index], controllers[(int)Mathf.Repeat(index + 1, controllers.Length)]);
            }

            if (Input.GetButtonDown("Jump_2"))
            {
                InitializePlayer();
            }
        }
    }

    private void InitializePlayer()
    {
        int index = (int)Mathf.Repeat(GetCurrentControllerIndex() + 1, controllers.Length);

        if (controllers[index].playerNumber == 0)
        {
            controllers[index].playerNumber = 2;
            controllers[index].CameraSettings.CameraReference.SetActive(true);
        }
    }

    private void SwitchPlayer(PlayerController2 oldController, PlayerController2 newController)
    {
        oldController.CameraSettings.CameraReference.SetActive(false);
        newController.CameraSettings.CameraReference.SetActive(true);

        oldController.playerNumber = 0;
        newController.playerNumber = 1;
    }

    private PlayerController2[] GetCurrentControllers()
    {
        return controllers.Where(c => c.playerNumber == 1).ToArray();
    }
    private PlayerController2 GetFirstController()
    {
        return GetCurrentControllers()[0];
    }
    private int GetCurrentControllerIndex()
    {
        return System.Array.FindIndex(controllers, c => c == GetFirstController());
    }
}
