﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : PlayerController2 {

    public bool isCharging;
    private bool chargeInterrupted;
    private CatChargeHitbox m_ChargeHitBox;

    public CatChargeHitbox ChargeHitBox
    {
        get
        {
            if (!m_ChargeHitBox)
            {
                m_ChargeHitBox = transform.Find("ChargeHitBox").GetComponent<CatChargeHitbox>();
            }

            return m_ChargeHitBox;
        }
    }

    Coroutine chargeReset;

    [SerializeField]
    private float ChargeSpeed = 8, ChargeTime = 1, chargeThreshold = 30, chargeRotationDamp = .4f;

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
            StartCoroutine(ChargeCoroutine());
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
    IEnumerator ChargeCoroutine()
    {
        float currentChargeTime = 0;
        isCharging = true;

        ChargeHitBox.gameObject.SetActive(true);

        while (isCharging)
        {
            if (currentChargeTime > ChargeTime)
                isCharging = false;

            currentChargeTime += Time.deltaTime;

            yield return new WaitForSeconds(Time.deltaTime);
        }

        ChargeHitBox.gameObject.SetActive(false);

    }

    protected override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);
        
        if ((controller.collisionFlags & CollisionFlags.CollidedSides) == CollisionFlags.CollidedSides)
        {
            isCharging = false;
        }
    }


    IEnumerator ResetCharge()
    {
        yield return null;
        isCharging = false;
    }
    public void IgnoreHit()
    {
        if (chargeReset != null) StopCoroutine(chargeReset);
    }
}