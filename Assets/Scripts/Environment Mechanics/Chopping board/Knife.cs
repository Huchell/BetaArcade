using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Knife : MonoBehaviour {

    #region variables

    [SerializeField]
    private float m_ChopXRotation = -60f;

    [Header("Speeds")]
    [SerializeField]
    private float m_ChopUpSpeedConst = 1f;
    [SerializeField]
    private float m_ChopDownSpeedConst = 5f;

    [Header("Delays")]
    [SerializeField]
    private float m_ChopDelay = 1f;
    [SerializeField]
    private float m_IdleDelay = 1f;

    private Coroutine choppingCoroutine;

    private bool m_Chop = false;
    private bool m_isChopping = false;
    public bool isChopping
    {
        get { return m_isChopping; }
    }

    public AudioClip m_ChopClip;
    AudioSource m_audioSource;

    #endregion


    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }
    #region Coroutine Callers

    public Coroutine ChopFully()
    {
        return StartCoroutine(ChopFully_Coroutine());
    }
    public Coroutine ChopUp()
    {
        return StartCoroutine(ChopUp_Coroutine());
    }
    public Coroutine ChopDown()
    {
        return StartCoroutine(ChopDown_Coroutine());
    }

    #endregion

    IEnumerator ChopFully_Coroutine()
    {
        m_Chop = true;
        WaitForSeconds
            wait_ChopDelay = new WaitForSeconds(m_ChopDelay),
            wait_IdleDelay = new WaitForSeconds(m_IdleDelay);

        while (m_Chop)
        {
            yield return ChopUp();



            yield return wait_ChopDelay;

            yield return StartCoroutine(Chop_Coroutine(transform.localEulerAngles.x, 0, m_ChopDownSpeedConst));

            yield return wait_IdleDelay;
        }
    }
    IEnumerator ChopUp_Coroutine()
    {
        yield return StartCoroutine(Chop_Coroutine(transform.localEulerAngles.x, m_ChopXRotation, m_ChopUpSpeedConst));

        if (m_ChopClip != null)
        {
            m_audioSource.clip = m_ChopClip;
            m_audioSource.Play();
        }
    }
    IEnumerator ChopDown_Coroutine()
    {
        yield return StartCoroutine(Chop_Coroutine(transform.localEulerAngles.x, 0, m_ChopDownSpeedConst));
    }
    IEnumerator Chop_Coroutine(float startXRotation, float endXRotation, float speed)
    {
        m_isChopping = true;

        float chopLerp = 0;

        while (chopLerp < 1)
        {
            RotateKnife(startXRotation, endXRotation, chopLerp);

            chopLerp += Time.deltaTime * speed;

            yield return new WaitForSeconds(Time.deltaTime);
        }

        RotateKnife(startXRotation, endXRotation, 1);

        m_isChopping = false;
    }
    IEnumerator Chop_Coroutine(float startXRotation, float endXRotation, AnimationCurve curve)
    {
        float chop_lerp = 0;

        while (chop_lerp < 1)
        {
            // Rotate the knife
            RotateKnife(startXRotation, endXRotation, chop_lerp);

            // Work out Speed
            float speed = curve.Evaluate(chop_lerp);

            chop_lerp += Time.deltaTime * speed;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    void RotateKnife(float startX, float endX, float lerp)
    {
        Vector3 euler = transform.localEulerAngles;

        euler.x = Mathf.LerpAngle(startX, endX, lerp);

        transform.localEulerAngles = euler;
    }
}
