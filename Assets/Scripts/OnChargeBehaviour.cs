using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnChargeBehaviour : MonoBehaviour {

    public UnityEvent OnCharge;

    void OnChargeHit(CatController controller)
    {
        //Debug.Log("Hit");
        OnCharge.Invoke();
    }
}
