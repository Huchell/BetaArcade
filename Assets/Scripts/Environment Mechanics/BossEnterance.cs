using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEnterance : MonoBehaviour {

    [SerializeField]
    private UnityEvent OnOpen;

    public void Check()
    {
        if (PlayerController2.ingredient2)
        {
            OnOpen.Invoke();
        }
    }
}
