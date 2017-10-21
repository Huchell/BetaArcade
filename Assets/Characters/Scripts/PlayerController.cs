using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	public float Speed = 7;
	public float JumpHeight = 5;
    public float energy = 1500.0f;
    public bool JumpOne = false;
	public bool JumpTwo = false;

	public bool CanJump = true;
    private bool IsSprint = false;

    private Rigidbody rb;
	public LayerMask groundLayers;
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.transform.tag == "Walkable") 
		{
			CanJump = true;
			JumpOne = false;
			JumpTwo = false;
 
            

        }

	}

	void FixedUpdate()
	{
		float x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
		float z = Input.GetAxis("Vertical") * Time.deltaTime * Speed;

		transform.Translate(x, 0, z);

        if (Input.GetButtonDown("Fire2"))
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
		

		if(Input.GetButtonDown("Jump"))
		{
			if (CanJump)
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
			}
		}
	}

	private void Jump()
	{
		rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
	}
}