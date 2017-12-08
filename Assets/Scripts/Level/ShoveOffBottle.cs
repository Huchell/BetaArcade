using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoveOffBottle : MonoBehaviour {

    public UnityEngine.Events.UnityEvent OnTrigger;

    private bool triggered = false;
    public void RabbitTrigger(Collider col)
    {
        if (!triggered)
        {
            try
            {
                CharacterController controller = (CharacterController)col;

                if (controller.isGrounded)
                {
                    OnTrigger.Invoke();
                    triggered = true;
                }
            }
            catch { }
            
        }
    }
}
