﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitController : PlayerController2 {

    private bool isDJump = false;
    private bool isChargeJumping = false;

    [SerializeField]
    private float 
        weakChargeThreshold = 20, 
        strongChargeThreshold = 60,
        weakJumpHeight = 2,
        strongJumpHeight = 4,
        chargeJumpAirControl = 1;

    protected override float GetCurrentSpeed(Vector3 direction)
    {
        float speed = base.GetCurrentSpeed(direction);

        if (isChargeJumping) speed *= chargeJumpAirControl;

        return speed;
    }
    protected override bool CanJumpCheck()
    {
        if (!isChargeJumping)
        if (controller.isGrounded)
        {
            return true;
        }
        else if (!isDJump)
        {
            isDJump = true;
            return true;
        }

        return false;
    }
    protected override void OnChargedAction()
    {
        if (chargeValue > strongChargeThreshold)
        {
            Debug.Log("Strong");
            Jump(strongJumpHeight);
        }
        else if (chargeValue > weakChargeThreshold)
        {
            Debug.Log("Weak");
            Jump(weakJumpHeight);
        }

        isChargeJumping = true;
    }
    protected override void OnGrounded()
    {
        base.OnGrounded();
        isDJump = false;
        isChargeJumping = false;
    }
}