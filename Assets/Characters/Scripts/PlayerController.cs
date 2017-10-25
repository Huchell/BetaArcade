using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	public float Speed = 7;
	public float JumpHeight = 5;
    public float GravityMultiplier = 2;
    public float energy = 1500.0f;
    public bool JumpOne = false;
	public bool JumpTwo = false;

	public bool CanJump = true;
    private bool IsSprint = false;

    private Rigidbody rb;
	public LayerMask groundLayers;

    private CapsuleCollider m_CapsuleCollider;

    public bool isGrounded
    {
        get
        {
            if (Physics.Raycast(transform.position, Vector3.down, (m_CapsuleCollider.height / 2) + 0.1f, groundLayers.value))
            {
                JumpTwo = false;
                return true;
            }
            else
                return false;
        }
    }
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
    }

	/*void OnCollisionEnter(Collision col)
	{
		if (col.transform.tag == "Walkable") 
		{
			CanJump = true;
			JumpOne = false;
			JumpTwo = false;
 
            

        }

	}*/

	void FixedUpdate()
	{
        float x = Input.GetAxisRaw("Horizontal") /* Time.deltaTime*/ * Speed;
        float z = Input.GetAxisRaw("Vertical") /* Time.deltaTime*/ * Speed;

        Debug.LogFormat("X: {0}, Z: {1}", x, z);

        rb.velocity = new Vector3(x, rb.velocity.y, z);
        //transform.Translate(x, 0, z);

        if (Input.GetButtonDown("Fire3"))
        {
            IsSprint = !IsSprint;
        }

        if (IsSprint == true && energy >= 0f)
        {
            Speed = 14;

                energy = energy - 10;
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


	void Update ()
	{
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                if (!JumpTwo)
                {
                    Jump();
                    JumpTwo = true;
                }
            }

            /*if (CanJump)
			{
				Jump ();
				if (!JumpOne && !JumpTwo) {
					JumpOne = true;
				} else if (JumpOne && !JumpTwo)
				{
					JumpTwo = true;
					JumpOne = false;
					CanJump = false;
				}
			}*/
        }


        ApplyDownForce();
	}

    private void ApplyDownForce()
    {
        if (!isGrounded)
            rb.AddForce((Physics.gravity * GravityMultiplier) - Physics.gravity);
    }

	private void Jump()
	{
        Vector3 velo = rb.velocity;
        velo.y = 0;
        rb.velocity = velo;

        rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
	}
}