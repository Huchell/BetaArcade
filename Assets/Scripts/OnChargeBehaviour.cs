using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnChargeBehaviour : MonoBehaviour {

    [SerializeField] private bool invokeCharge = true;
    [SerializeField] private bool forceChargeToStop = false;

    public UnityEvent OnCharge;
    

    public bool InvokeCharge
    {
        get
        {
            return invokeCharge;
        }
        set
        {
            invokeCharge = value;
        }
    }

    void OnChargeHit(CatController controller)
    {
        if (InvokeCharge)
        {
            if (forceChargeToStop)
            {
                controller.StopCharge();
            }

            OnCharge.Invoke();

        }
    }
}
