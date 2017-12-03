using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnChargeBehaviour : MonoBehaviour {

    [System.Serializable]
    public class ChargeCallback : UnityEvent<ControllerColliderHit> { }
    public ChargeCallback OnCharge;

    void OnChargeHit(ControllerColliderHit hit)
    {
        OnCharge.Invoke(hit);
    }
}
