using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBoxNodeData : MonoBehaviour
{
    public enum collisionSide { North, East, South, West };

    [Tooltip("North is Z+, East is X+, South is Z-, West is X+")]
    public collisionSide sideToPushTo, sideToPushAway;
    [Tooltip("Use me for sliding/falling")]
    public bool autoMoveToNextNode = false;

    public void SuggestPushes(Transform previousNode)
    {
        Debug.Log(previousNode.position.x + ", " + previousNode.position.z);
        Debug.Log(transform.position.x + ", " + transform.position.z);

        float xDifference = previousNode.position.x - transform.position.x;
        float zDifference = previousNode.position.z - transform.position.z;

        Debug.Log(xDifference + ", " + zDifference);

        if (Mathf.Abs(xDifference) >= Mathf.Abs(zDifference))
        {
            if (xDifference > 0)
            {
                sideToPushTo = collisionSide.West;
                sideToPushAway = collisionSide.East;
                Debug.Log("node is to West " + xDifference);
            }
            else
            {
                sideToPushTo = collisionSide.East;
                sideToPushAway = collisionSide.West;
                Debug.Log("node is to East " + xDifference);
            }
        }
        else
        {
            if (zDifference > 0)
            {
                sideToPushTo = collisionSide.South;
                sideToPushAway = collisionSide.North;
                Debug.Log("node is to South " + zDifference);
            }
            else
            {
                sideToPushTo = collisionSide.North;
                sideToPushAway = collisionSide.South;
                Debug.Log("node is to North " + zDifference);
            }
        }
    }
}