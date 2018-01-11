using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class Mover : MonoBehaviour
{
    public Rail rail;
    public PlayMode mode;

    public float speed = 2.5f;
    public bool isReversed;
    public bool isLooping;
    public bool pingPong;

    private int currentSeg;
    private float transition;
    private bool isCompleted;

    private int count = 0;
    public float countDelay = 0;

    private void FixedUpdate()
    {
        if (!rail)
        {
            return;
        }
        if (!isCompleted)
        {
            Play(!isReversed);
        }
    }

    private void Play(bool forward = true)
    {
        if (count > countDelay)
        {
            float m = (rail.nodes[currentSeg + 1].position - rail.nodes[currentSeg].position).magnitude;
            float s = (Time.deltaTime * 1 / m) * speed;
            transition += (forward) ? s : -s;

            if (transition > 1)
            {
                transition = 0;
                currentSeg++;
                if (currentSeg == rail.nodes.Length - 1)
                {
                    if (isLooping)
                    {
                        if (pingPong)
                        {
                            transition = 1;
                            currentSeg = rail.nodes.Length - 2;
                            isReversed = !isReversed;
                        }
                        else
                        {
                            currentSeg = 0;
                        }
                    }
                    else
                    {
                        isCompleted = true;
                        return;
                    }
                }
            }
            else if (transition < 0)
            {
                transition = 1;
                currentSeg--;
                if (currentSeg == -1)
                {
                    if (isLooping)
                    {
                        if (pingPong)
                        {
                            transition = 0;
                            currentSeg = 0;
                            isReversed = !isReversed;
                        }
                        else
                        {
                            currentSeg = rail.nodes.Length - 2;
                        }
                    }
                    else
                    {
                        isCompleted = true;
                        return;
                    }
                }
            }

            transform.position = rail.PositionOnRail(currentSeg, transition, mode);
            transform.rotation = rail.Orientation(currentSeg, transition);
        }
        count++;
    }
}
