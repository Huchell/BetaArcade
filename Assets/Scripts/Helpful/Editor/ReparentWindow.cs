using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class ReparentWindow : EditorWindow {

    [MenuItem("Tools/Reparent")]
    public static void Create()
    {
        ReparentWindow rw = CreateInstance<ReparentWindow>();
        rw.name = "Reparent";
        rw.Show();
    }

    GameObject[] selectedGameObjects;

    private void OnSelectionChange()
    {
        selectedGameObjects = Selection.gameObjects.Where(g => g.activeInHierarchy==true).ToArray();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Reparent"))
        {
            Reparent();
        }
    }

    public void Reparent()
    {
        Vector3 parentPosition = new Vector3();

        for (int i = 0; i < selectedGameObjects.Length; i++)
        {
            parentPosition.x += selectedGameObjects[i].transform.position.x;
            parentPosition.y += selectedGameObjects[i].transform.position.y;
            parentPosition.z += selectedGameObjects[i].transform.position.z;
        }

        parentPosition.x /= 2;
        parentPosition.y /= 2;
        parentPosition.z /= 2;

        GameObject parent = new GameObject("Parent");
        parent.transform.position = parentPosition;
        
        for (int i = 0; i < selectedGameObjects.Length; i++)
        {
            selectedGameObjects[i].transform.SetParent(parent.transform, true);
        }
    }
}
