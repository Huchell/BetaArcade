using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Player Controllers/Cat")]
public class CatController : PlayerController2 {

    public bool isCharging;
    private bool chargeInterrupted;

    [SerializeField]
    private float ChargeSpeed = 8, ChargeTime = 1, chargeThreshold = 30, chargeRotationDamp = .7f;

    protected override bool CanJumpCheck()
    {
        // Prevent the player from jumping when charging
        if (!isCharging)
            return base.CanJumpCheck();
        else
            return false;
    }
    protected override void OnChargedAction()
    {
        if (chargeValue >= chargeThreshold)
            StartCoroutine(Charge());
    }
    protected override float GetCurrentSpeed(Vector2 direction)
    {
        if (isCharging)
            return ChargeSpeed;
        else
            return base.GetCurrentSpeed(direction);
    }
    protected override float GetRotationDamp()
    {
        return isCharging ? chargeRotationDamp : base.GetRotationDamp();
    }

    IEnumerator Charge()
    {
        float currentChargeTime = 0;
        isCharging = true;
        
        while (isCharging)
        {
            if (currentChargeTime > ChargeTime)
                isCharging = false;

            currentChargeTime += Time.deltaTime;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isCharging)
        {
            if (hit.gameObject.CompareTag("Destructible"))
            {

                hit.gameObject.GetComponent<DestructibleMesh>().
                    DestructObject(100, hit.gameObject.transform.position, 10);
            }
            else
            {
                RaycastHit rayHit;
                if (controller.Raycast(new Ray(transform.position + controller.center, transform.forward), out rayHit, 0.1f))
                {
                    if (rayHit.transform.gameObject == hit.gameObject)
                    {
                        hit.gameObject.SendMessage("OnChargeHit", hit, SendMessageOptions.DontRequireReceiver);

                        // Hit in front
                        isCharging = false;
                    }   
                }
            }
        }
    }
}
