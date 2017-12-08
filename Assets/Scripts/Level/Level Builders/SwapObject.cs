using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapObject : MonoBehaviour {

    [SerializeField] private GameObject swapPrefab;

    [Header("Transform queries")]
    [SerializeField] private bool keepParent = true;
    [SerializeField] private bool keepPosition = true;
    [SerializeField] private bool keepRotation = true;
    [SerializeField] private bool keepScale = true;

    [Space]
    [Header("Offsets")]
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private Vector3 scaleOffset;

	public void Swap()
    {
        if (swapPrefab)
        {
            // Get Spawn pos, rot, and scl
            Vector3 spawnPos = keepPosition ? transform.position : Vector3.zero;
            Vector3 spawnRot = keepRotation ? transform.eulerAngles : Vector3.zero;
            Vector3 spawnScl = keepScale ? transform.localScale : Vector3.one;

            // Apply offsets
            spawnPos += positionOffset;
            spawnRot += rotationOffset;
            spawnScl += scaleOffset;

            GameObject newGameObject;
            if (keepParent)
                newGameObject = Instantiate(swapPrefab, spawnPos, Quaternion.Euler(spawnRot), transform.parent);
            else
                newGameObject = Instantiate(swapPrefab, spawnPos, Quaternion.Euler(spawnRot));

            newGameObject.transform.localScale = spawnScl;
        }

        Destroy(gameObject);
    }
}
