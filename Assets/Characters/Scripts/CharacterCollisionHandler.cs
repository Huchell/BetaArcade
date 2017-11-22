using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterCollisionHandler : MonoBehaviour {

    private struct CollisionScope
    {
        public CharacterController controller;
        public bool colliding;

        public CollisionScope(CharacterController controller, bool colliding)
        {
            this.controller = controller;
            this.colliding = colliding;
        }
    }

    private List<CollisionScope> collisions = new List<CollisionScope>();

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("Hit");
        if (collisions.Where(cs => cs.controller == hit.controller).ToArray().Length >= 1)
        {
            hit.gameObject.SendMessage("OnCharacterCollisionStay", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            collisions.Add(new CollisionScope(hit.controller, true));
            hit.gameObject.SendMessage("OnCharacterCollisionEnter", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void LateUpdate()
    {
        foreach (CollisionScope scope in collisions)
        {
            CharacterController controller = scope.controller;
            Vector3 
                point1 = transform.position + controller.center + new Vector3(0, controller.height / 2, 0),
                point2 = point1 + new Vector3(0, controller.height / 2, 0);
            if (Physics.CapsuleCast(point1, point2, controller.radius + controller.skinWidth + 0.1f, Vector3.zero))
            {
                Debug.Log("Test");
            }
        }
    }
}
