using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	public float Speed = 7;
	public float JumpHeight = 5;
    public float GravityMultiplier = 2;
    public float energy = 1500.0f;
	public bool JumpTwo = false;
    public float Strength = 0;
    private bool IsSprint = false;
    public bool Charging = false;

    private Rigidbody rb;
	public LayerMask groundLayers;

    private CapsuleCollider m_CapsuleCollider;

    [ReadOnly][SerializeField]private float m_lastGroundCheckTime;

    private Vector3 m_prevPosition;
    private Ray m_GroundRay;
    private Ray GroundRay
    {
        get
        {
            if (transform.position != m_prevPosition)
                m_GroundRay = new Ray(transform.position, Vector3.down);

            return m_GroundRay;
        }
    }

    private RaycastHit m_groundHit;
    public RaycastHit GroundHitInfo
    {
        get
        {
            return m_groundHit;
        }
    }

    /// <summary>
    /// Used to cache the isGrounded check
    /// </summary>
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
                m_isGrounded = (Physics.Raycast(GroundRay, out m_groundHit, (m_CapsuleCollider.height / 2) + 0.1f, groundLayers.value));
            }

            // If you are grounded, then reset the double jump
            if (m_isGrounded)
            {
                JumpTwo = false;
            }

            // return the is grounded boolean
            return m_isGrounded;
        }
    }

	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
    }

	void FixedUpdate()
	{
		float x = Input.GetAxis("Horizontal") * Speed;
		float z = Input.GetAxis("Vertical") * Speed;

        rb.velocity = new Vector3(x, rb.velocity.y, z);

        /*
         * TODO: Change hard coded values into variables
         */
        if (Input.GetButtonDown("Fire3"))
        {
            IsSprint = !IsSprint;
        }
        if (IsSprint == true && energy >= 0f)
        {
            Speed = 14;

            if(isGrounded)
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
    void Update()
    {

        /*
         * TODO: Change hard coded values into variables 
         */
        if (Input.GetButtonDown("Jump"))
        {
            if (Strength >= 60)
            {
                JumpTwo = true;
            }
            Strength = 0;
            Charging = !Charging;
        }

        if (Input.GetButtonUp("Jump"))
        {
            if (Strength < 60)
            {
                if (isGrounded)
                {
                    Jump();
                }
                else if (!JumpTwo)
                    {
                        Jump();
                        JumpTwo = true;
                    }
            }
            if (Strength >= 60)
            {
                ChargeJump(Strength);
            }
            Charging = false;
        }

        // Increase the charging value or sets it to 0
        if (Charging)
        {
            Strength = Strength + 1;
        }
        else
        {
           //Strength = 0;
        }

        //stops the player charging a jump in the air
        if (Charging && !isGrounded)
        {
            Charging = false;
        }

        ApplyDownForce();
	}

    private void ApplyDownForce()
    {
        if (!isGrounded)
        {
            rb.AddForce((Physics.gravity * GravityMultiplier) - Physics.gravity);
        }
    }

	private void Jump()
	{
        Vector3 velo = rb.velocity;
        velo.y = 0;
        rb.velocity = velo;
        rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
	}

    private void ChargeJump(float strength)
    {
        Vector3 velo = rb.velocity;
        velo.y = 0;
        rb.velocity = velo;
        if (strength < 60)
        {
            strength = JumpHeight;
        }
        else if(strength >= 60)
        {
            strength = JumpHeight*1.7f;
        }
        rb.AddForce(Vector3.up * strength, ForceMode.Impulse);
    }
}