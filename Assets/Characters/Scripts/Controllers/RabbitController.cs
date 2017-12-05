using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Player Controllers/Rabbit")]
public class RabbitController : PlayerController2 {

    //[SerializeField][ReadOnly]
    //private bool isDJump = false;
    private bool isChargeJumping = false;

    [SerializeField]
    private float 
        weakChargeThreshold = 20, 
        strongChargeThreshold = 60,
        weakJumpHeight = 2,
        strongJumpHeight = 4,
        chargeJumpAirControl = 1;

    protected override float GetCurrentSpeed(Vector2 direction)
    {
        float speed = base.GetCurrentSpeed(direction);

        if (isChargeJumping) speed *= chargeJumpAirControl;

        return speed;
    }
    protected override bool CanJumpCheck()
    {
        if (!isChargeJumping)
            return base.CanJumpCheck();
        else 
            return false;
    }

    protected override void OnChargedAction()
    {
        if (chargeValue > strongChargeThreshold)
        {
            Jump(strongJumpHeight, false);
        }
        else if (chargeValue > weakChargeThreshold)
        {
            Jump(weakJumpHeight, false);
        }

        isChargeJumping = true;
    }
    protected override void OnGrounded()
    {
        base.OnGrounded();
        //isDJump = false;
        isChargeJumping = false;
    }
}
