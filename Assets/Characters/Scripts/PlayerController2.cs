using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour {

    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;
    public float m_InAirControlMultiplier = 1;
    public float chargeValue = 0;
    public float m_ChargeJumpMultiplier = 1;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    bool isDJump;
    bool isCharge = false;
    bool isChargeJump = false;
    bool canMove = true;

    private float currentWalkSpeed;

    public int playerNumber = 0;
    public PlayerType characterType;

    Transform cameraT;
    CharacterController controller;
    Animator animator;

    [HideInInspector]
    private PlayerCameraSettings m_CameraSettings;
    public PlayerCameraSettings CameraSettings
    {
        get
        {
            if (!m_CameraSettings)
            {
                m_CameraSettings = GetComponent<PlayerCameraSettings>();
            }

            return m_CameraSettings;
        }
    }

	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
        cameraT = CameraSettings.CameraReference.transform;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {

        if (playerNumber > 0)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw(GetInputString("Horizontal")), Input.GetAxisRaw(GetInputString("Vertical")));
            Vector2 inputDir = input.normalized;

            if (canMove)
            {
                //bool running = Input.GetButton("Sprint");
                bool running = Mathf.Abs(Input.GetAxis(GetInputString("Sprint"))) > 0;
                float targetSpeed = ((running) ? runSpeed : walkSpeed) * 
                    (controller.isGrounded ? 1 : 
                        isChargeJump ? m_ChargeJumpMultiplier : m_InAirControlMultiplier) * 
                    inputDir.magnitude;

                currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
            }
            else
            {
                currentSpeed = 0;
            }

            if (inputDir != Vector2.zero) //stops 0/0 errors
            {
                float targetRot = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref turnSmoothVelocity, turnSmoothTime); //Character rotation
            }

            if (Input.GetButtonDown(GetInputString("Jump")))
            {
                Jump();
            }

            if (Input.GetButtonDown(GetInputString("Charge Jump")))
            {
                canMove = false;
                isCharge = true;
            }

            if (isCharge)
            {
                chargeValue++;
            }

            if (Input.GetButtonUp(GetInputString("Charge Jump")))
            {
                isDJump = true;
                isChargeJump = true;

                if (chargeValue > 20 && chargeValue < 60)
                {
                    jumpHeight = 2;
                    Jump();
                }
                else if (chargeValue > 60)
                {
                    jumpHeight = 4;
                    Jump();
                }
                jumpHeight = 1;
                isCharge = false;
                chargeValue = 0;
                canMove = true;
            }
        }
        else
        {
            if (controller.isGrounded)
                currentSpeed = 0;
        }

        velocityY += Time.deltaTime * gravity;
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if(controller.isGrounded)
        {
            isDJump = false;
            isChargeJump = false;
            velocityY = 0;
        }

        UpdateAnimations();
    }
    void Jump()
    {
        if(controller.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); //Jump equation
            velocityY = jumpVelocity;
        }
        if (!controller.isGrounded && !isDJump)
        {
            isDJump = true;
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    void UpdateAnimations()
    {
        if (animator && animator.runtimeAnimatorController != null)
        {
            bool isMoving = currentSpeed > 0;
            animator.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                animator.SetFloat("Speed", currentSpeed);
            }
        }
    }

    public string GetInputString(string input)
    {
        return input + "_" + playerNumber;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }
}
