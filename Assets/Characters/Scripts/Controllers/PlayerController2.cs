using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController), typeof(PlayerCameraSettings))]
public class PlayerController2 : MonoBehaviour {

    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;
    public float chargeValue = 0;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    bool isCharge = false;
    bool canMove = true;

    public int playerNumber = 0;
    public PlayerType characterType;

    protected Transform cameraT;
    protected CharacterController controller;
    protected Animator animator;

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

    private bool groundedPrevFrame = false;

	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
        cameraT = CameraSettings.CameraReference.transform;
        animator = GetComponent<Animator>();

        groundedPrevFrame = controller.isGrounded;
    }

    // Update is called once per frame
    void Update() {

        if (playerNumber > 0)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw(GetInputString("Horizontal")), Input.GetAxisRaw(GetInputString("Vertical")));
            Vector2 inputDir = input.normalized;

            currentSpeed = GetCurrentSpeed(inputDir);

            if (inputDir != Vector2.zero) //stops 0/0 errors
            {
                float targetRot = Mathf.Atan2(inputDir.x, inputDir.y) * GetRotationDamp() * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref turnSmoothVelocity, turnSmoothTime); //Character rotation
            }

            if (Input.GetButtonDown(GetInputString("Jump")))
            {
                Jump(jumpHeight);
            }

            if (controller.isGrounded)
            {
                #region Charge
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
                    OnChargedAction();

                    // Reset Charge variables 
                    // and make it so the player can move again
                    chargeValue = 0;
                    isCharge = false;
                    canMove = true;
                }
                #endregion
            }
        }
        else
        {
            if (controller.isGrounded)
                currentSpeed = 0;
        }

        // Apply Gravity
        velocityY += Time.deltaTime * gravity;

        // Workout velocity
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        // Move the player
        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        // Check to see if the controller has just been grounded
        if(controller.isGrounded && !groundedPrevFrame)
        {
            OnGrounded();
        }

        UpdateAnimations();

        // Set groundedPrevFrame to isGrounded at the end of 
        // everything to get ready for the next frame
        groundedPrevFrame = controller.isGrounded;
    }

    #region Methods

    #region Move
    protected virtual float GetCurrentSpeed(Vector2 direction)
    {
        // If the player can move, work out the current speed
        if (canMove)
        {
            bool running = Mathf.Abs(Input.GetAxis(GetInputString("Sprint"))) > 0;
            float targetSpeed = ((running) ? runSpeed : walkSpeed) * direction.magnitude;

            return Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
        }

        // else set the speed to 0
        return 0;
    }
    protected virtual float GetRotationDamp()
    {
        return 1;
    }
    #endregion
    #region Jump
    protected void Jump(float height)
    {
        if(CanJumpCheck())
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * height); //Jump equation
            velocityY = jumpVelocity;
        }
    }
    protected virtual bool CanJumpCheck()
    {
        return controller.isGrounded;
    }
    protected virtual void OnGrounded()
    {
        isCharge = false;
        velocityY = 0;
    }
    #endregion
    #region Charge
    protected virtual void OnChargedAction() { throw new System.NotImplementedException(); }
    #endregion
    protected virtual void UpdateAnimations()
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

    #region Util
    public string GetInputString(string input)
    {
        return input + "_" + playerNumber;
    }
    #endregion
    #endregion
}
