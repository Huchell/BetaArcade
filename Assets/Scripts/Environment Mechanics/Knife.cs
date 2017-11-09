using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Knife : MonoBehaviour {

    public Animator anim;

    public bool isChopping = false;
    public float choppingDelay = 5f;

    private float time = 0f;

    private bool playingLastFrame = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Chop()
    {
        anim.SetBool("Chop", true);
    }
}
