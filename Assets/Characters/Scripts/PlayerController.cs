using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    public int health = 3;
    public bool CanMove = true;
    public float Speed = 7;
    public float JumpHeight = 5;
    public float GravityMultiplier = 2;
    public float energy = 1500.0f;
    public bool JumpTwo = false;
    public float JumpStrength = 0;
    private bool IsSprint = false;
    public bool Charging = false;
    public LayerMask groundLayers;
    public float RotationSpeed = 250f;
    public bool CanJump = true;
    public int flipHorizontal = 1;

    private bool m_isMoving = false;
    public bool isMoving
    {
        get { return m_isMoving; }
    }

    [SerializeField]
    private int m_playerNumber = -1;

    public int playerNumber
    {
        get
        {
            return m_playerNumber;
        }
        set
        {
            m_playerNumber = value;
            if (m_playerNumber < 0)
            {
                CanMove = false;
                CanJump = false;
                camera.gameObject.SetActive(false);
            }
            else
            {
                CanMove = true;
                CanJump = true;
                camera.gameObject.SetActive(true);
            }
        }
    }

    [Range(0, 179)]
    [SerializeField] float maxPitch = 30;
    [Range(-179, 0)]
    [SerializeField] float minPitch = -30;

    #region Component References
    private Rigidbody rb;
    private CapsuleCollider m_CapsuleCollider;
    [HideInInspector]new public Camera camera;
    private Transform cameraArm;
    private Animator m_Animator;
    #endregion

    #endregion

    #region Is Grounded Property

    #region GroundedEvent

    [System.Serializable]
    public class PlayerGroundedEvent : UnityEvent<RaycastHit> { }

    public PlayerGroundedEvent OnGrounded;

    #endregion

    #region Ray

    private Vector3 m_prevPosition;
    private Ray m_GroundRay;
    private Ray GroundRay
    {
        get
        {
            if (transform.position != m_prevPosition)
            {
				m_GroundRay = new Ray(transform.position + new Vector3(
                    m_CapsuleCollider.center.x * transform.lossyScale.x,
                    m_CapsuleCollider.center.y * transform.lossyScale.y,
                    m_CapsuleCollider.center.z * transform.lossyScale.z),
                    Vector3.down
                    );

				Debug.DrawRay (m_GroundRay.origin, m_GroundRay.direction, Color.white, 1f);
                m_prevPosition = transform.position;
            }

            return m_GroundRay;
        }
    }

    #endregion

    #region RaycastHit
    private RaycastHit m_groundHit;
    public RaycastHit GroundHitInfo
    {
        get
        {
            return m_groundHit;
        }
    }
    #endregion

    #region The Actual Raycast
    private bool m_groundedLastFrame;
    [ReadOnly] [SerializeField] private float m_lastGroundCheckTime;

    /// <summary>
    /// Used to cache the isGrounded check
    /// </summary>
    [ReadOnly] [SerializeField]
    private bool m_isGrounded;
    /// <summary>
    /// Property finds out if the player is grounded
    /// </summary>
    public bool isGrounded
    {
        get
        {
            // Check to see if the current time is past the last time you 
            // checked the ground and the time for 1 frame
            if (Time.time > m_lastGroundCheckTime + Time.deltaTime)
            {
                // Set the last time checked to the current time
                m_lastGroundCheckTime = Time.time;

                // Perform the grounded raycast
                m_isGrounded = (Physics.Raycast(GroundRay, out m_groundHit, ((m_CapsuleCollider.height * transform.lossyScale.y) / 2) +0.1f, groundLayers.value));
            }

            // If you are grounded, then reset the double jump
            if (!m_groundedLastFrame && m_isGrounded)
            {
                JumpTwo = false;
                rb.velocity = Vector3.zero;
                m_Animator.SetBool("isJumping", false);
            }

            m_groundedLastFrame = m_isGrounded;

            // return the is grounded boolean
            return m_isGrounded;
        }
    }
    #endregion

    #endregion

    void Start ()
	{
        // Get Components
		rb = GetComponent<Rigidbody> ();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        cameraArm = transform.Find("CameraArm");
        camera = cameraArm.GetComponentInChildren<Camera>();

        m_Animator = GetComponentInChildren<Animator>();

        if (playerNumber < 0)
        {
            CanMove = false;
            CanJump = false;
            camera.gameObject.SetActive(false);
        }
        else
        {
            CanMove = true;
            CanJump = true;
            camera.gameObject.SetActive(true);
            camera.targetDisplay = playerNumber;
        }

        //transform.position = SaveBox.Load();
    }

    void FixedUpdate()
	{
        // If the player can move, move them
        if (CanMove)
        {
            ApplyMovement();

            // Horizontal camera Rotation
            float MouseDeltaX = Input.GetAxis(GetInputStringFromPlayer("Mouse X"));

            if (MouseDeltaX != 0)
            {
                /*if (isMoving)
                {
                    transform.Rotate(Vector3.up, MouseDeltaX * Time.deltaTime * RotationSpeed * flipHorizontal, Space.Self);
                }
                else
                {*/
                    cameraArm.transform.Rotate(Vector3.up, MouseDeltaX * Time.deltaTime * RotationSpeed * flipHorizontal, Space.World);
                //}
            }

            // Vertical Camera Rotation
            float MouseDeltaY = Input.GetAxis(GetInputStringFromPlayer("Mouse Y"));

            if (MouseDeltaY != 0)
            {
                Quaternion rotationDelta = cameraArm.transform.rotation;
                Vector3 eulerRotationDelta = rotationDelta.eulerAngles;

                float newx = eulerRotationDelta.x + MouseDeltaY * Time.deltaTime * RotationSpeed * -1;

                if (newx < 180)
                    newx = Mathf.Min(newx, Mathf.Repeat(maxPitch, 360));
                else
                    newx = Mathf.Max(newx, Mathf.Repeat(minPitch, 360));

                eulerRotationDelta.x = newx;

                rotationDelta.eulerAngles = eulerRotationDelta;
                cameraArm.transform.rotation = rotationDelta;
            }

            if (Input.GetButtonDown(GetInputStringFromPlayer("Fire3")))
            {
                IsSprint = !IsSprint;
            }

            if (IsSprint && energy >= 0f)
            {
                Speed = 14;

                if (isGrounded)
                {
                    energy = energy - 10;
                }
            }
            else if (energy < 1500)
            {
                Speed = 7;
                energy = energy + 5;
                if (energy == 0)
                {
                    IsSprint = false;
                }
            }
        }

        if (CanJump)
        {
            // Handle Jumping Logic
            JumpButtonDown();
            JumpButtonUp();
        }

        // Apply Gravity
        ApplyDownForce();

        // Increase the charging value or sets it to 0
        if (Charging)
        {
            JumpStrength = JumpStrength + 1;
        }
	}

    public void OnDamage(int dmg)
    {
        health -= dmg;
        Debug.Log(health);
        switch (health)
        {
            case 3:
                {
                    //LeftEarUp
                    //RightEarUp
                    break;
                }

            case 2:
                {
                    //LeftEarDown
                    //RightEarUp
                    break;
                }


            case 1:
                {
                    //LeftEarDown
                    //RightEarDown
                    break;
                }
                
            case 0:
                {
                    //DeathSequence
                    break;
                }
        }
    }

    #region Movement
    void ApplyMovement()
    {
        float x_axis = Input.GetAxis(GetInputStringFromPlayer("Horizontal")), z_axis = Input.GetAxis(GetInputStringFromPlayer("Vertical"));
        
        float x = x_axis * Speed * Time.deltaTime;

        float z = z_axis * Speed * Time.deltaTime;

        //transform.Translate(x, 0, z);
        rb.MovePosition(rb.position + transform.TransformVector(new Vector3(x, 0, z)));

        m_isMoving = (x != 0 || z != 0);

        //m_Animator.applyRootMotion = !isMoving;

        m_Animator.SetFloat("Z_Axis", z_axis);
        m_Animator.SetBool("isMoving", isMoving);
    }

    #region Jump Methods

    /// <summary>
    /// Logic that handles the Jump Button Down
    /// </summary>
    void JumpButtonDown()
    {
        if (Input.GetButtonDown(GetInputStringFromPlayer("Jump")))
        {
            if (JumpStrength >= 20)
            {
                JumpTwo = true;
            }
            JumpStrength = 0;

            if (isGrounded)
                Charging = true;
        }
    }
    /// <summary>
    /// Logic that handles the Jump Button Up
    /// </summary>
    void JumpButtonUp()
    {
        if (Input.GetButtonUp(GetInputStringFromPlayer("Jump")))
        {
            if (JumpStrength < 20)
            {
                if (isGrounded)
                {
                    Jump();
                    JumpTwo = false;
                }
                else if (!JumpTwo)
                {
                    Jump();
                    JumpTwo = true;
                }
            }
            if (JumpStrength >= 20)
            {
                ChargeJump(JumpStrength);
            }
            Charging = false;
        }
    }

    private void Jump()
    {
        Jump(JumpHeight);
    }
    private void Jump(float strength)
    {
        // Reset the Y velocity of the player for the jump
        // needed for the double jump
        Vector3 velo = rb.velocity;
        velo.y = 0;
        rb.velocity = velo;

        rb.AddForce(Vector3.up * strength, ForceMode.Impulse);

        m_Animator.SetBool("isJumping", true);
    }

    private void ChargeJump(float strength)
    {
        if (strength < 60)
        {
            if (strength >= 20 && strength < 59)
            {
                strength = JumpHeight * 1.4f;
            }
            else
            {
                strength = JumpHeight;
            }
        }
        else if (strength >= 60)
        {
            strength = JumpHeight * 1.7f;
        }

        Jump(strength);
    }
    #endregion

    private void ApplyDownForce()
    {
        if (!isGrounded)
        {
            rb.AddForce((Physics.gravity * GravityMultiplier) - Physics.gravity);
        }
    }
    #endregion

    private string GetInputStringFromPlayer(string startString)
    {
        return startString + "_" + playerNumber.ToString();
    }
}