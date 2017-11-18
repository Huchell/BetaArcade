using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingBoard : MonoBehaviour {

    public Knife[] knives;
    public bool m_ChopOnStart = true;

    [SerializeField]
    private Knife knifeObjectPrefab;

    [Header("Delays")]
    [SerializeField]
    private float m_upDelay = 0f;
    [SerializeField]
    private float m_holdDelay = 0f;
    [SerializeField]
    private float m_downDelay = 0f;
    [SerializeField]
    private float m_choppedDelay = 0f;

    private Transform m_KnifeHolder;
    public Transform KnifeHolder
    {
        get
        {
            if (!m_KnifeHolder)
            {
                m_KnifeHolder = transform.Find("KnifeHolder");
                if (!m_KnifeHolder)
                {
                    m_KnifeHolder = new GameObject("KnifeHolder").transform;
                }
            }

            return m_KnifeHolder;
        }
    }

    private void Start()
    {
        if (m_ChopOnStart)
            ChopAllKnives();
    }

    public void ChopAllKnives()
    {
        StartCoroutine(ChopAllKnives_Coroutine());
    }

    private IEnumerator ChopAllKnives_Coroutine()
    {
        WaitWhile knivesCutting = new WaitWhile(AreknivesCutting);
        while (true)
        {
            for (int i = 0; i < knives.Length; i++)
            {
                knives[i].ChopUp();
                yield return new WaitForSeconds(m_upDelay);
            }

            yield return knivesCutting;

            yield return new WaitForSeconds(m_holdDelay);

            for (int i = 0; i < knives.Length; i++)
            {
                knives[i].ChopDown();
                yield return new WaitForSeconds(m_downDelay);
            }

            yield return knivesCutting;

            yield return new WaitForSeconds(m_choppedDelay);
        }
    }

    private bool AreknivesCutting()
    {
        bool returnBool = false;

        foreach (Knife k in knives)
        {
            if (k.isChopping)
            {
                returnBool = true;
                break;
            }
        }

        return returnBool;
    }

    public void ChopKnife(int index)
    {
        if (index > 0 && index < knives.Length)
        {
            knives[index].ChopFully();
        }
    }


    #region Initialize Knives

    public void AddKnife()
    {
        Knife[] newArray = new Knife[knives.Length + 1];

        for (int i = 0; i < knives.Length; i++)
            newArray[i] = knives[i];

        newArray[newArray.Length - 1] = CreateKnifeObject();
        knives = newArray;
    }
    private Knife CreateKnifeObject()
    {
        Knife newKnife;

        if (knifeObjectPrefab)
        {
            newKnife = Instantiate(knifeObjectPrefab, KnifeHolder);

        }
        else
        {
            GameObject gm = new GameObject("Knife", typeof(Knife));
            new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer)).transform.SetParent(gm.transform);
            gm.transform.SetParent(transform.Find("KnifeHolder"));

            newKnife = gm.GetComponent<Knife>();
        }

        newKnife.transform.localPosition = Vector3.zero;
        newKnife.transform.localRotation = Quaternion.identity;

        return newKnife;
    }

    public void RemoveKnife(int index, bool destroyKnife = true)
    {
        if (index < 0 || index >= knives.Length)
            return;

        Knife removeKnife = knives[index];

        Knife[] newArray = new Knife[knives.Length - 1];

        for (int i = 0, j = 0;  i < knives.Length; i++)
            if (i != index)
            {
                newArray[j] = knives[i];
                j++;
            }

        knives = newArray;

        if (destroyKnife)
        {
            if (Application.isPlaying)
                Destroy(removeKnife.gameObject);
            else
                DestroyImmediate(removeKnife.gameObject);
        }
    }

    #endregion
}
