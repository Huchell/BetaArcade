using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour {

    [Name("Mesh Transform")]
    [Tooltip("The Transform of the object that has the knife mesh.\nIf null defaults to this object")]
    public Transform knifeMesh;

    [SerializeField] MinMax m_XLean;
    [SerializeField] float m_XLeanSpeed = 5;

    [ReadOnly]
    [SerializeField]
    float m_CurrentXLean;

    [SerializeField] MinMax m_ZLean;
    [SerializeField] float m_ZLeanSpeed = 5;

    [ReadOnly]
    [SerializeField]
    float m_CurrentZLean;

    float m_PrevZPosition;

    Coroutine m_LeaningCoroutine;

    private void Awake()
    {
        if (!knifeMesh) knifeMesh = this.transform;
    }

    public void MoveKnife(Vector3 newPosition)
    {
        transform.position = newPosition;

        if (m_LeaningCoroutine == null)
            StartLeanKnife(m_XLean.Max, m_XLeanSpeed);
    }
    public Coroutine StartCut()
    {
        return StartLeanKnife(m_XLean.Min, m_XLeanSpeed);
    }

    IEnumerator LeanKnife(float targetX, float speed)
    {
        Quaternion rotation = knifeMesh.rotation;
        Vector3 eulerRotation = rotation.eulerAngles;

        int direction = (int)Mathf.Sign(targetX - eulerRotation.x);

        while ((int)Mathf.Sign(targetX - eulerRotation.x) == direction)
        {
            eulerRotation.x = IncreaseXLean(direction, targetX, speed);
            //eulerRotation.z = IncreaseZLean(direction, targetZ);

            rotation.eulerAngles = eulerRotation;
            knifeMesh.rotation = rotation;

            yield return new WaitForEndOfFrame();
        }
    }

    public Coroutine StartLeanKnife(float targetX, float speed)
    {
        if (m_LeaningCoroutine != null)
            StopCoroutine(m_LeaningCoroutine);

        m_LeaningCoroutine = StartCoroutine(LeanKnife(targetX, speed));

        return m_LeaningCoroutine;
    }

    float IncreaseXLean(int direction, float target, float speed)
    {
        return m_CurrentXLean = IncreaseLean(m_CurrentXLean, speed, direction, target);
    }
    float IncreaseZLean(int direction, float target, float speed)
    {
        return m_CurrentZLean = IncreaseLean(m_CurrentZLean, speed, direction, target);
    }

    float IncreaseLean(float currentLean, float Speed, int direction, float target)
    {
        currentLean += Time.deltaTime * Speed * direction;

        switch (direction)
        {
            case 1:
                if (currentLean >= target)
                    currentLean = target;
                break;
            case -1:
                if (currentLean <= target)
                    currentLean = target;
                break;
            default:
                Debug.Log("YOU DONE FUCKED UP A-ARON!");
                break;
        }

        return currentLean;
    }
}
