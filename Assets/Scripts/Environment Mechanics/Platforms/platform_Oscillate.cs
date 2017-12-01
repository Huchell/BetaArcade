using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platform_Oscillate : MonoBehaviour
{
	[Tooltip("Prefab used for platform and ghosts. You must click 'Refresh Platforms' to make this change.")]
	public GameObject mesh;
	[Tooltip("If unchecked, 'ghost' platforms will disappear if the offset is very close to 0.0 or 1.0.")]
	public bool AlwaysShowGhosts = true;
	[Tooltip("Platform scale.")]
	public Vector3 selfScale = new Vector3(1.0f, 1.0f, 1.0f);
	[Tooltip("Movement speed of platform.")]
	public float speed = 1.0f;
	[Tooltip("This variable is a multiplier of wait time. 1.0 would mean the platform will stop and wait at the start and end positions for 1.0x the time it takes to travel between the two positions.")]
	public float waitRatio = 0.1f; //this waits an extra 10% of the journey at the 'stops'
	private MeshFilter mf;

	public PhysicMaterial phyMaterial;

    public bool isActive = true;

	[HideInInspector]
	public GameObject startPosition, endPosition, platform;

	[Range(0.0f, 1.0f)]
	[Tooltip("0 is at start position; 1 is at end position.")]
	public float offset = 0.5f;

	float timer = 0.0f;

	// Use this for initialization
	void Start ()
	{
		mf = GetComponent<MeshFilter> ();
		mf.mesh = null;
		startPosition.GetComponent<MeshRenderer> ().enabled = false;
		endPosition.GetComponent<MeshRenderer> ().enabled = false;
	}

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            timer += Time.deltaTime;
            float stdTime = timer * (speed / (Vector3.Distance(startPosition.transform.position, endPosition.transform.position)));
            float offsetSync = stdTime + ((((offset * 2f) - 1f) * Mathf.PI) * 0.5f);
            float lerpValue = (Mathf.Sin(offsetSync) + 1f) / 2f;
            //Debug.Log (lerpValue);
            lerpValue = Mathf.Clamp(((lerpValue * (1.0f + (2 * waitRatio))) - waitRatio), 0.0f, 1.0f); //this is to add more 'stop' at the edges

            float lerpX = ((1 - lerpValue) * startPosition.transform.position.x) + (lerpValue * endPosition.transform.position.x);
            float lerpY = ((1 - lerpValue) * startPosition.transform.position.y) + (lerpValue * endPosition.transform.position.y);
            float lerpZ = ((1 - lerpValue) * startPosition.transform.position.z) + (lerpValue * endPosition.transform.position.z);
            Vector3 lerpPosition = new Vector3(lerpX, lerpY, lerpZ);

            platform.transform.position = lerpPosition;
            //Debug.Log (lerpPosition);
        }
    }

    public void ToggleActivity()
    {
        isActive = !isActive;
    }

    public void RefreshPlatforms()
	{
		startPosition.GetComponent<MeshFilter> ().mesh = endPosition.GetComponent<MeshFilter> ().mesh = mesh.GetComponent<MeshFilter> ().sharedMesh;

		GameObject temp = Instantiate (mesh, transform);
		DestroyImmediate (platform);
		Debug.Log (temp);
		platform = temp;
		platform.GetComponent<Collider>().material = phyMaterial;
	}
}
