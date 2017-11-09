using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ChoppingBoard : MonoBehaviour {

	private enum ChoppingState {
		Idle,
		Tracking,
		Cutting
	}

    [Tooltip("The transform of the knife the chopping board will be using")]
	[SerializeField] Knife m_Knife;

    [Tooltip("The height off the board the knife sits at rest")]
	[SerializeField] float m_KnifeYOffset;

    [Tooltip("The speed at which the knife tracks the player")]
    [SerializeField] float m_KnifeSpeed = 2.0f;

    [Tooltip("The distance from the player the knife has to be before it starts cutting")]
    [Range(0, 1)] [SerializeField] float m_KnifeDistanceTolerence;

    [ReadOnly]
    [Tooltip("The Target for the Knife")]
    [SerializeField] Transform m_Target;

    [Tooltip("The amount of time it will shake for")]
	[SerializeField] float m_ShakeTime = 1.0f;

    [Tooltip("How much the shake vector is amplified by")]
	[SerializeField] float m_ShakeAmplitude = 1.0f;

    [Tooltip("The speed at which the Knife cuts down at")]
    [SerializeField] float m_CuttingSpeed = 2.0f;

    [Tooltip("The Cooldown time for the knife before it starts tracking the player after a cut")]
    [SerializeField] float m_CooldownTime = 1.0f;

    [SerializeField] bool m_RandomSway = false;

    [SerializeField] AnimationCurve m_SwayCurve;
    [ReadOnly] [SerializeField] float m_CurrentSwayTime;
    int m_CurrentSwayDirection = 1;

    float m_SwayCurveEndTime
    {
        get
        {
            return m_SwayCurve[m_SwayCurve.length - 1].time;
        }
    }

	ChoppingState m_chopState = ChoppingState.Idle;
	float m_currentShakeTime = 0.0f;


    BoxCollider c_BoxCollider;
	WaitForFixedUpdate wait;

	void Awake()
	{
		wait = new WaitForFixedUpdate ();
        c_BoxCollider = GetComponent<BoxCollider>();
	}

    #region Trigger Messages

    void OnTriggerEnter(Collider col) {
		if (col.CompareTag ("Player")) {

			// If Target is not set then set it
			if (!m_Target)
				SetTarget (col.transform, true);
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.CompareTag ("Player")) {

			// If the object is the target remove it
			if (m_Target == col.transform) {

                RemoveTarget();
				ChangeChoppingState (ChoppingState.Idle);
			}
		}
	}

#endregion

    void TrackTarget() {

		if (m_Knife == null)
			return;

		Vector3 thisPos = m_Knife.transform.position;
		Vector3 targetPos = m_Target.position;
        thisPos.y = targetPos.y = m_KnifeYOffset;

        Vector3 moveVector = Vector3.MoveTowards(thisPos, targetPos, Time.deltaTime * m_KnifeSpeed);

        m_Knife.MoveKnife(moveVector);
	}

	void CheckCutTarget() {
		Vector3 thisPos = m_Knife.transform.position;
		Vector3 targetPos = m_Target.position;

		if (NearlyEqual(thisPos.x, targetPos.x, m_KnifeDistanceTolerence) && NearlyEqual(thisPos.z, targetPos.z, m_KnifeDistanceTolerence)) {

			ChangeChoppingState (ChoppingState.Cutting);
		}
	}
		
	#region State Coroutines

	IEnumerator Tracking() {
		while (m_Target && m_chopState == ChoppingState.Tracking) {

			TrackTarget ();

			CheckCutTarget ();

			yield return wait;
		}
	}
	IEnumerator StartCutting() {

        yield return m_Knife.StartCut();

		Vector3 startPos = m_Knife.transform.position;
        float currentShakeTime = 0;

		while (currentShakeTime < m_ShakeTime) {
			Vector3 randomVector = Random.insideUnitSphere;
			m_Knife.transform.position = startPos + randomVector * m_ShakeAmplitude;

			currentShakeTime += Time.deltaTime;

			yield return wait;
		}

        m_Knife.transform.position = startPos;

        
		bool hasCut = false;
		int direction = 1;
		float cutAmount = 0;

        m_Knife.StartLeanKnife(0, .1f);

		while (!hasCut)
		{
            Debug.Log("Cut");
			Vector3 knifePos = m_Knife.transform.position;

			cutAmount = Mathf.Clamp (cutAmount + (Time.deltaTime * m_CuttingSpeed * direction), 0, m_KnifeYOffset);
			knifePos.y = m_KnifeYOffset - cutAmount;

			m_Knife.transform.position = knifePos;

			if (cutAmount == m_KnifeYOffset)
				direction = -1;

			hasCut = cutAmount == 0;

            yield return wait;
		}

        yield return new WaitForSeconds(m_CooldownTime);

        ChangeChoppingState(ChoppingState.Idle);
	}

	#endregion

	void ChangeChoppingState(ChoppingState state)
	{
		m_chopState = state;

		switch (m_chopState) {
		case ChoppingState.Idle:
			break;
		case ChoppingState.Cutting:
			StartCoroutine (StartCutting ());
			break;
		case ChoppingState.Tracking:
			StartCoroutine (Tracking ());
			break;
		}
	}

	void OnValidate() {

        if (m_Knife)
        {
            Vector3 knifePos = m_Knife.transform.position;
            knifePos.y = m_KnifeYOffset;
            m_Knife.transform.position = knifePos;
        }
	}

	bool NearlyEqual(float a, float b, float tolerance = 0.000001f) {
		return (a + tolerance >= b && a - tolerance <= b) || (b + tolerance >= a && b - tolerance <= a);
	}

	public void SetTarget(Transform target, bool trackTarget = false)
	{
		m_Target = target;

		if (trackTarget)
			ChangeChoppingState (ChoppingState.Tracking);
	}
	public void RemoveTarget() {
		m_Target = null;
	}
}
