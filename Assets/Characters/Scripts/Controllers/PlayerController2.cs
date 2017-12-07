using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController), typeof(PlayerCameraSettings))]
public class PlayerController2 : MonoBehaviour {

    public static bool ingredient1, ingredient2, ingredient3;

    private static int m_ingredientCounter;
    public static int ingredientCounter
    {
        get { return m_ingredientCounter; }
        set
        {
            m_ingredientCounter = value;
            if (m_ingredientCounter == 1)
            {
                ingredient1 = true;
            }
            else if (m_ingredientCounter == 2)
            {
                ingredient2 = true;
            }
            else if (m_ingredientCounter == 3)
            {
                ingredient3 = true;
            }
        }
    }

    #region GodMode
    [SerializeField]
    private bool GodMode = false;
    public void SetGodMode(bool value)
    {
        GodMode = value;
    }
    public void ToggleGodMode()
    {
        GodMode = !GodMode;
    }
    #endregion

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
    [SerializeField][ReadOnly]
    float velocityY;
    [SerializeField][ReadOnly]
    bool ChargingUp = false;

    public bool canMove = true;
    private bool lockSpeed = false;

    public bool canCharge = true;
    public bool canJump = true;

    private Vector2 moveInput;
    private Vector2 direction;
    protected float targetSpeed;
    private bool sprintKeyDown = false;

    public int playerNumber = 0;
    public bool playerActive = false;


    #region Components
    protected Transform cameraT;
    protected CharacterController controller;
    protected Animator animator;
    #endregion

    #region Animation Properties
    private bool isMoving
    {
        get
        {
            return direction != Vector2.zero;
        }
    }
    private bool isWalking
    {
        get
        {
            return isMoving && !sprintKeyDown;
        }
    }
    private bool isJumping
    {
        get
        {
            return velocityY > 0;
        }
    }
    private bool isFalling
    {
        get
        {
            return velocityY < 0 && !controller.isGrounded;
        }
    }
    private bool isSprinting
    {
        get
        {
            return isMoving && sprintKeyDown;
        }
    }
    #endregion

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

    public ParticleSystem FootstepParticles;
    private bool canFootstep = true;
    [SerializeField]
    private float footstepCooldown = 0.25f;
    [SerializeField]
    private AudioClip FootstepClip;

    private bool groundedPrevFrame = false;

	// Use this for initialization
	protected virtual void Start () {
        controller = GetComponent<CharacterController>();
        cameraT = CameraSettings.CameraReference.transform;
        animator = GetComponent<Animator>();

        groundedPrevFrame = controller.isGrounded;
    }

    // Update is called once per frame
    void Update() {

        RefreshMovement();
        GetCurrentTargetSpeed(direction);

        if (canMove)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

            if (direction != Vector2.zero) //stops 0/0 errors
            {
                float targetRot = Mathf.Atan2(direction.x, direction.y) * GetRotationDamp() * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref turnSmoothVelocity, turnSmoothTime); //Character rotation
            }
        } else { currentSpeed = 0; }

        if (canJump)
        {
            JumpInput();
        }

        if (controller.isGrounded)
        {
            if (canCharge)
            {
                ChargeInput();
            }
            else
            {
                ChargingUp = false;
            }
        }

        if (ChargingUp)
        {
            if (chargeValue == 0)
            {
                if (animator) animator.SetTrigger("ChargeStart");
            }

            chargeValue++;
        }

        ApplyGravity();
        MovePlayer();
        CheckGrounded();
        UpdateAnimations();

        // Set groundedPrevFrame to isGrounded at the end of 
        // everything to get ready for the next frame
        groundedPrevFrame = controller.isGrounded;
    }

    #region Methods

    #region Input Methods
    private void RefreshMovement()
    {
        if (playerActive)
        {
            moveInput = new Vector2(Input.GetAxisRaw(GetInputString("Horizontal")), Input.GetAxisRaw(GetInputString("Vertical")));
            sprintKeyDown = Mathf.Abs(Input.GetAxis(GetInputString("Sprint"))) > 0;
        }
        else
        {
            moveInput = Vector2.zero;
            sprintKeyDown = false;
        }

        direction = moveInput.normalized;
    }
    private void JumpInput()
    {
        if (playerActive)
        if (Input.GetButtonDown(GetInputString("Jump")))
        {
            Jump(jumpHeight);
        }
    }
    private void ChargeInput()
    {
        if (playerActive)
        {
            if (Input.GetButtonDown(GetInputString("Charge Jump")))
            {
                canMove = false;
                targetSpeed = 0;
                chargeValue = 0;
                ChargingUp = true;
            }

            if (Input.GetButtonUp(GetInputString("Charge Jump")))
            {
                Charge();
            }
        }
    }
    #endregion

    #region Move
    public void ApplyGravity()
    {
        // Apply Gravity
        if (!controller.isGrounded)
            velocityY += Time.deltaTime * gravity;
    }
    private void MovePlayer()
    {
        // Workout velocity
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        // Move the player
        controller.Move(velocity * Time.deltaTime);
        //currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
    }
    private void CheckGrounded()
    {
        // Check to see if the controller has just been grounded
        if (controller.isGrounded && !groundedPrevFrame)
        {
            OnGrounded();
        }
    }
    protected virtual void GetCurrentTargetSpeed(Vector2 direction)
    {
        // If the player can move, work out the current speed
        if (playerActive)
        {
            if (canMove)
                targetSpeed = ((isSprinting && (!isJumping && !isFalling)) ? runSpeed : walkSpeed) * direction.magnitude;
        }
        else
        {
            targetSpeed = 0;
        }
    }
    protected virtual float GetRotationDamp()
    {
        return 1;
    }
    #endregion
    #region Jump
    public void Jump(float height, bool triggerAnim = true)
    {
        if(GodMode || CanJumpCheck())
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * height); //Jump equation
            velocityY = jumpVelocity;


            if (triggerAnim && animator) animator.SetTrigger("Jump");
        }
    }
    protected virtual bool CanJumpCheck()
    {
        return controller.isGrounded;
    }
    protected virtual void OnGrounded()
    {
        ChargingUp = false;
        lockSpeed = false;
    }
    #endregion
    #region Charge
    public void Charge()
    {
        // Reset Charge variables 
        // and make it so the player can move again
        ChargingUp = false;
        canMove = true;

        OnChargedAction();
    }
    public virtual void StopCharge()
    {
        chargeValue = 0;
    }
    protected virtual void OnChargedAction() { throw new System.NotImplementedException(); }
    #endregion

    protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (controller.collisionFlags == CollisionFlags.Above) { velocityY = 0; }
    }

    protected virtual void UpdateAnimations()
    {
        if (animator && animator.runtimeAnimatorController != null)
        {
            animator.SetBool("isMoving", isMoving);

            if (isMoving)
            {
                
                animator.SetFloat("Speed", currentSpeed);
            }
            else
            {
                animator.SetFloat("Speed", 0);
            }

            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isSprinting", isSprinting);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isFalling", isFalling);
            animator.SetBool("isCharging", ChargingUp);
            animator.SetFloat("ChargeAmount", chargeValue / 120);

            if (!controller.isGrounded && groundedPrevFrame && !isJumping && velocityY < -5)
                animator.SetTrigger("FellOff");
        }
    }

    #region Util
    public string GetInputString(string input)
    {
        return input + "_" + playerNumber;
    }
    void Footstep()
    {
        if (FootstepParticles)
        {
            Transform footstepSpawn = transform.Find("FootstepSpawn");
            if (!footstepSpawn) footstepSpawn = transform;

            Instantiate(FootstepParticles, footstepSpawn.position, footstepSpawn.rotation).transform.localScale = footstepSpawn.lossyScale;
        }
    }
    public void SetPlayer(int index)
    {
        playerNumber = index;
        playerActive = playerNumber > 0;
        CameraSettings.CameraReference.SetActive(playerActive);
    }
    #endregion
    #endregion
}
