using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyLerp : MonoBehaviour {

    [System.Serializable]
    public struct LerpData
    {
        [System.Serializable]
        public enum PropertyType
        {
            Color,
            Float,
            Vector2,
            Vector3,
            Vector4
        }

        public string name;
        public PropertyType type;
        public float duration;
        public float speed;

        public Color toColor;
        public float toFloat;
        public Vector2 toVector2;
        public Vector3 toVector3;
        public Vector4 toVector4;
    }

    public bool m_CreateInstance = true;
    public LerpData[] data;
    public UnityEngine.Events.UnityEvent OnLerpFinished;

    private Material m_Material;
    private Material material
    {
        get
        {
            if (!m_Material)
            {
                if (m_CreateInstance)
                    m_Material = GetComponent<MeshRenderer>().material;
                else
                    m_Material = GetComponent<MeshRenderer>().sharedMaterial;
            }

            return m_Material;
        }
    }

    public void LerpAll()
    {
        foreach (LerpData d in data)
        {
            Lerp(d);
        }
    }
    public void Lerp(string name)
    {
        name.Replace(" ", "");

        if (name[0] != '_') name = "_" + name;

        LerpData lerp;
        if (HasName(name, out lerp))
        {
            Lerp(lerp);
        }
    }
    void Lerp(LerpData data)
    {
        if (material.HasProperty(data.name))
        {
            switch (data.type)
            {
                case LerpData.PropertyType.Color:
                    StartCoroutine(LerpCoroutine(material.GetColor(data.name), data.toColor, data));
                    break;
                case LerpData.PropertyType.Float:
                    StartCoroutine(LerpCoroutine(material.GetFloat(data.name), data.toFloat, data));
                    break;
                case LerpData.PropertyType.Vector2:
                    StartCoroutine(LerpCoroutine(material.GetVector(data.name), data.toVector2, data));
                    break;
                case LerpData.PropertyType.Vector3:
                    StartCoroutine(LerpCoroutine(material.GetVector(data.name), data.toVector3, data));
                    break;
                case LerpData.PropertyType.Vector4:
                    StartCoroutine(LerpCoroutine(material.GetVector(data.name), data.toVector4, data));
                    break;
            }
        }
    }

    IEnumerator LerpCoroutine(Color color1, Color color2, LerpData data)
    {
        float lerpTime = 0;

        while (lerpTime < data.duration)
        {
            lerpTime += Time.deltaTime * data.speed;

            material.SetColor(
                data.name,
                Color.Lerp(color1, color2, lerpTime)
                );

            yield return null;
        }

        OnLerpFinished.Invoke();
    }
    IEnumerator LerpCoroutine(float float1, float float2, LerpData data)
    {
        float lerpTime = 0;

        while (lerpTime < data.duration)
        {
            lerpTime += Time.deltaTime * data.speed;

            material.SetFloat(
                data.name,
                Mathf.Lerp(float1, float2, lerpTime / data.duration)
                );

            yield return null;
        }

        OnLerpFinished.Invoke();
    }
    IEnumerator LerpCoroutine(Vector4 vector1, Vector4 vector2, LerpData data)
    {
        float lerpTime = 0;

        while (lerpTime < data.duration)
        {
            lerpTime += Time.deltaTime * data.speed;

            material.SetVector(
                data.name,
                Vector4.Lerp(vector1, vector2, lerpTime / data.duration)
                );

            yield return null;
        }

        OnLerpFinished.Invoke();
    }

    bool HasName(string name, out LerpData data)
    {
        foreach (LerpData d  in this.data)
        {
            if (d.name == name)
            {
                data = d;
                return true;
            }
        }

        data = new LerpData();
        return false;
    }
}
