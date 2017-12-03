using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class LevelTriggerBehaviour : MonoBehaviour {

    [SerializeField] private bool catNeeded = true;
    [SerializeField] private bool rabbitNeeded = true;

    private bool catIn = false;
    private bool rabbitIn = false;

    [Serializable]
    public class LevelTrigger : UnityEvent<Collider> { }

    public LevelTrigger OnEnter;
    public LevelTrigger OnStay;
    public LevelTrigger OnExit;

	private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2 controller = other.GetComponent<PlayerController2>();

            if (controller)
            {
                try
                {
                    CatController cat = (CatController)controller;

                    catIn = true;
                }
                catch
                {
                    try
                    {
                        RabbitController rabbit = (RabbitController)controller;
                        rabbitIn = true;
                    }
                    catch
                    {
                        Debug.LogError("Player doesnt exist!");
                    }
                }
                finally
                {
                    Trigger(OnEnter, other);
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        Trigger(OnStay, other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2 controller = other.GetComponent<PlayerController2>();

            if (controller)
            {
                try
                {
                    CatController cat = controller as CatController;

                    catIn = false;
                }
                catch
                {
                    try
                    {
                        RabbitController rabbit = controller as RabbitController;
                        rabbitIn = false;
                    }
                    catch
                    {
                        Debug.LogError("Player doesnt exist!");
                    }
                }
                finally
                {
                    Trigger(OnExit, other);
                }
            }
        }

    }

    private void Trigger(LevelTrigger e, Collider other)
    {
        if (catNeeded == catIn && rabbitNeeded == rabbitIn)
        {
            e.Invoke(other);
        }
    }
}
