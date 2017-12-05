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
        if (GetCurrentControllers().Length == 1)
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
            controllers[index].SetPlayer(2);

            SplitScreen();
        }
    }

    void SplitScreen()
    {
        PlayerController2[] conts = GetCurrentControllers();

        conts[0].CameraSettings.CameraReference.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
        conts[1].CameraSettings.CameraReference.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
    }
    private void SwitchPlayer(PlayerController2 oldController, PlayerController2 newController)
    {
        oldController.SetPlayer(0);
        newController.SetPlayer(1);
    }

    private PlayerController2[] GetCurrentControllers()
    {
        return controllers.Where(c => c.playerNumber > 0).ToArray();
    }
    private PlayerController2 GetFirstController()
    {
        return GetCurrentControllers()[0];
    }
    private int GetCurrentControllerIndex()
    {
        return System.Array.FindIndex(controllers, c => c == GetFirstController());
    }

    public void StopAllPlayers()
    {
        foreach(PlayerController2 controller in GetCurrentControllers())
        {
            controller.canMove = false;
            controller.canJump = false;
            controller.canCharge = false;

            controller.CameraSettings.CameraReference.enabled = false;
        }
    }
    public void StartAllPlayers()
    {
        foreach (PlayerController2 controller in GetCurrentControllers())
        {
            controller.canMove = true;
            controller.canJump = true;
            controller.canCharge = true;

            controller.CameraSettings.CameraReference.enabled = true;
        }
    }
}
