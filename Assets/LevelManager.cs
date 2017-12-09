using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LevelManager : MonoBehaviour {

    private static LevelManager instance;
    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("LevelManager").AddComponent<LevelManager>();
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public string[] Scenes;

    private void Start()
    {
        LoadScenes();
    }

    public void LoadScenes()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            foreach (string s in Scenes)
            {
                if (!EditorSceneManager.GetSceneByName(s).isLoaded)
                    EditorSceneManager.OpenScene("Assets/Scenes/Level Parts/" + s + ".unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
            }
            return;
        }
#endif

        foreach (string s in Scenes)
        {
            if (!SceneManager.GetSceneByName(s).isLoaded)
                SceneManager.LoadScene(s, LoadSceneMode.Additive);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private LevelManager castedTarget;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Load Scenes"))
        {
            castedTarget = (target as LevelManager);

            castedTarget.LoadScenes();
        }
    }
}
#endif
