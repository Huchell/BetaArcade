using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Player Controllers/Rabbit")]
public class RabbitController : PlayerController2 {

    [SerializeField][ReadOnly]
    private bool isDJump = false;
    [SerializeField][ReadOnly]
    private bool isChargeJumping = false;

    [SerializeField]
    private float 
        weakChargeThreshold = 20, 
        strongChargeThreshold = 60,
        weakJumpHeight = 2,
        strongJumpHeight = 4,
        chargeJumpAirControl = 1;

    protected override void GetCurrentTargetSpeed(Vector2 direction)
    {
        base.GetCurrentTargetSpeed(direction);

        if (isChargeJumping) targetSpeed *= chargeJumpAirControl;
    }
    protected override bool CanJumpCheck()
    {
        if (!isChargeJumping)
        {
            if (base.CanJumpCheck())
            {
                return true;
            }
            else if (!isDJump)
            {
                isDJump = true;
                return true;
            }
        }

        return false;
    }

    protected override void OnChargedAction()
    {
        if (chargeValue > strongChargeThreshold)
        {
            Jump(strongJumpHeight, false);
            isChargeJumping = true;
        }
        else if (chargeValue > weakChargeThreshold)
        {
            Jump(weakJumpHeight, false);
            isChargeJumping = true;
        }
    }
    protected override void OnGrounded()
    {
        base.OnGrounded();
        isDJump = false;
        isChargeJumping = false;
    }
}
