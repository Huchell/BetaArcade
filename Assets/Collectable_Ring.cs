using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable_Ring : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] collectables;
    public GameObject prefab;
    public int buttonCount;
    public float radius = 5f;

    public void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void RefreshButtons()
    {
        for (int x = transform.childCount - 1; x >= 0; x--)
        {
            DestroyImmediate(transform.GetChild(x).gameObject);
        }

        collectables = new GameObject[buttonCount];

        for (int count = 0; count < buttonCount; count++)
        {
            Vector3 offset = Quaternion.AngleAxis((360f / buttonCount) * count, Vector3.up) * new Vector3(0, 0, radius);
            GameObject coin = Instantiate(prefab);
            coin.transform.SetParent(transform, false);
            coin.transform.localPosition += offset;
            coin.transform.RotateAround(coin.transform.position, Vector3.up, (360f / buttonCount) * count);
            coin.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            coin.transform.parent = transform;
            collectables[count] = coin;
        }
    }
}