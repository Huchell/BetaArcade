using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platform_Ring_Update : MonoBehaviour
{

    [Tooltip("Number of platforms. Capped at 32 for stability; see Adam if this cap is insufficient. You must click 'Refresh Platforms' to make this change.")]
    public int numberOfPlatforms;
    [HideInInspector]
    public GameObject[] platforms;
    [Tooltip("Radius of ring of platforms. You must click 'Refresh Platforms' to make this change.")]
    public float radius = 5f;
    [Tooltip("Prefab used for platform and ghosts. You must click 'Refresh Platforms' to make this change.")]
    public GameObject platformType;
    [Tooltip("Platform scale.")]
    public Vector3 platformScale = new Vector3(1.0f, 1.0f, 1.0f);

    private int platformCountCap = 32;

    private bool update = true;

    [Tooltip("Rotations per second. 0.1f means it will take 10 seconds for one full rotation.")]
    public float RPS = 0.1f;
    private MeshFilter mf;
    private float worldTime = 0f;

    [Tooltip("Height that platforms will rise and fall for.")]
    public float heightOffset = 50.0f;

    [Tooltip("Speed that platforms will rise and fall.")]
    public float riseSpeed = 1.0f;

    public bool isActive = true;

    public void Start()
    {
        mf = GetComponent<MeshFilter>();
        mf.mesh = null;
        if (isActive) { StartCoroutine("RingUpdate"); }
    }

    public void Update()
    {
        /*
        if (isActive)
        {
            transform.RotateAround(transform.position, Vector3.up, 360f * RPS * Time.deltaTime);

            worldTime += Time.deltaTime;

            if (heightOffset != 0 && riseSpeed != 0)
            {
                for (int x = 0; x < platforms.Length; x++)
                {
                    float ratio = (float)x / (float)(platforms.Length);
                    float timeMod = worldTime * riseSpeed / heightOffset;

                    float yOffsetAmount = Mathf.Sin((timeMod + ratio) * 2 * Mathf.PI);

                    platforms[x].gameObject.transform.position = new Vector3(platforms[x].transform.position.x, (transform.position.y) + (yOffsetAmount), platforms[x].transform.position.z);
                }
            }
        }
        */
    }

    IEnumerator RingUpdate()
    {
        while (isActive)
        {
            transform.RotateAround(transform.position, Vector3.up, 360f * RPS * Time.deltaTime);

            worldTime += Time.deltaTime;

            if (heightOffset != 0 && riseSpeed != 0)
            {
                for (int x = 0; x < platforms.Length; x++)
                {
                    float ratio = (float)x / (float)(platforms.Length);
                    float timeMod = worldTime * riseSpeed / heightOffset;

                    float yOffsetAmount = Mathf.Sin((timeMod + ratio) * 2 * Mathf.PI);

                    platforms[x].gameObject.transform.position = new Vector3(platforms[x].transform.position.x, (transform.position.y) + (yOffsetAmount), platforms[x].transform.position.z);
                }
            }
            yield return null;
        }
    }


    public void ToggleActivity()
    {
        isActive = !isActive;
        if (isActive){ StartCoroutine("RingUpdate"); }
    }

    public void OnValidate()
    {
        numberOfPlatforms = Mathf.Clamp(numberOfPlatforms, 0, platformCountCap);
    }

    IEnumerator DestroyPlatforms(GameObject platform)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(platform);
    }

    public void RefreshPlatforms()
    {
        for (int x = transform.childCount - 1; x >= 0; x--)
        {
            DestroyImmediate(transform.GetChild(x).gameObject);
        }

        platforms = new GameObject[numberOfPlatforms];

        for (int count = 0; count < numberOfPlatforms; count++)
        {
            Vector3 offset = Quaternion.AngleAxis((360f / numberOfPlatforms) * count, Vector3.up) * new Vector3(0, 0, radius);
            GameObject gm = Instantiate(platformType);
            gm.transform.SetParent(transform, false);
            gm.transform.localPosition += offset;
            gm.transform.RotateAround(gm.transform.position, Vector3.up, (360f / numberOfPlatforms) * count);
            gm.transform.localScale = platformScale;
            gm.transform.parent = transform;

            platforms[count] = gm;
        }
    }

    public void AdjustPlatformScale()
    {
        if (platforms != null)
        {
            foreach (GameObject gm in platforms)
            {
                gm.transform.localScale = platformScale;
            }
        }
    }
}
