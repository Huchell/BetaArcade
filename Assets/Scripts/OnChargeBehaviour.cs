using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnChargeBehaviour : MonoBehaviour {

    public UnityEvent OnCharge;
    [SerializeField] private bool invokeCharge = true;

    public bool InvokeCharge
    {
        get
        {
            return invokeCharge;
        }
        set
        {
            InvokeCharge = value;
        }
    }

    void OnChargeHit(CatController controller)
    {
        if (InvokeCharge)
            //Debug.Log("Hit");
            OnCharge.Invoke();
    }
}
