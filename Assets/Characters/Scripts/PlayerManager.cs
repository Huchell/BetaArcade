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
        if (Input.GetKeyDown(KeyCode.P))
        {
            int index = GetCurrentControllerIndex();

            SwitchPlayer(controllers[index], controllers[(int)Mathf.Repeat(index + 1, controllers.Length)]);
        }

        if (Input.GetButtonDown("Jump_2"))
        {
            InitializePlayer();
        }
    }

    private void InitializePlayer()
    {
        int index = (int)Mathf.Repeat(GetCurrentControllerIndex() + 1, controllers.Length);

        if (controllers[index].playerNumber == 0)
        {
            controllers[index].playerNumber = GetFirstController().playerNumber + 1;

            GameObject camObj = new GameObject("Camera");
            ThirdPersonCamera tpc = camObj.AddComponent<ThirdPersonCamera>();
            tpc.player = controllers[index];
            tpc.target = controllers[index].transform.Find("CameraLookAt");

            controllers[index].camera = tpc;
        }
    }

    private void SwitchPlayer(PlayerController2 oldController, PlayerController2 newController)
    {
        newController.camera = oldController.camera;

        Transform lookAt = newController.transform.Find("CameraLookAt");
        newController.camera.target = lookAt ? lookAt : newController.transform;
        newController.camera.player = newController;

        newController.playerNumber = oldController.playerNumber;

        oldController.playerNumber = 0;
        oldController.camera = null;
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
