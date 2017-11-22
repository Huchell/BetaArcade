using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour {

	void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CharacterCollisionHandler cch = hit.gameObject.GetComponent<CharacterCollisionHandler>();
        if (cch != null)
        {
        }
    }
}
