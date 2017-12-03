using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScene : MonoBehaviour {

    Camera CutsceneCamera;

    private Camera oldCamera;
    [SerializeField] private float time;

    private Coroutine cutsceneCoroutine;

    private void Start()
    {
        CutsceneCamera = GetComponent<Camera>();
        CutsceneCamera.enabled = false;
    }

    public void StartCutscene()
    {
        //Turn off players cameras
        PlayerManager.Instance.StopAllPlayers();

        //Start Animation

        cutsceneCoroutine = StartCoroutine(cutscene());
    }

    public void StopCutscene()
    {
        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
            CutsceneCamera.enabled = false;
            PlayerManager.Instance.StartAllPlayers();
        }
        
    }

    IEnumerator cutscene()
    {
        CutsceneCamera.enabled = true;

        yield return new WaitForSeconds(time);

        EndCutscene();
    }

    void EndCutscene()
    {
        CutsceneCamera.enabled = false;

        PlayerManager.Instance.StartAllPlayers();
    }
}
