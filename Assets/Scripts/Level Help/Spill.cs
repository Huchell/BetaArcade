using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spill : MonoBehaviour {

    public Material NormalMaterial;
    public Material InertMaterial;

    [SerializeField]
    private bool debug;
    [SerializeField]
    [Range(0, 1)]
    private float lerpValue;

    private MeshRenderer m_renderer;
    public new MeshRenderer renderer
    {
        get
        {
            if (!m_renderer) m_renderer = GetComponent<MeshRenderer>();
            return m_renderer;
        }
    }

    private MeshCollider m_collider;
    public new MeshCollider collider
    {
        get
        {
            if (!m_collider) m_collider = GetComponent<MeshCollider>();
            return m_collider;
        }
    }

    private void Start()
    {
        lerpValue = 0;
        UpdateMaterial(lerpValue);
    }

    public void MakeSpillInert()
    {
        StartCoroutine(FullMaterialLerp());
    }

    private IEnumerator FullMaterialLerp()
    {
        while (lerpValue < 1)
        {
            UpdateMaterial(lerpValue);
            lerpValue += Time.deltaTime * .1f;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        collider.isTrigger = false;
    }

    public void UpdateMaterial(float t)
    {
        /*if (t >= 1)
            renderer.sharedMaterial = InertMaterial;
        else if (t <= 0)
            renderer.sharedMaterial = NormalMaterial;
        else
        {*/
            Material newMaterial = renderer.material;
            newMaterial.Lerp(NormalMaterial, InertMaterial, t);
            renderer.material = newMaterial;
        //}
    }
}
