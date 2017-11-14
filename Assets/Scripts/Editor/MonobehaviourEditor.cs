using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class MonobehaviourEditor : Editor {

    private readonly string[] methodButtons = new string[] { "OnConstruction" };

    public override void OnInspectorGUI()
    {
        CreateButtonsForMethods();

        DrawDefaultInspector();
    }

    void CreateButtonsForMethods()
    {
        MethodInfo[] infoArray = target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        for (int i = 0; i < infoArray.Length; i++)
        {
            MethodInfo info = infoArray[i];

            for(int y = 0; y < methodButtons.Length; y++)
            {
                if (info.Name == methodButtons[y])
                {
                    if (GUILayout.Button(methodButtons[y]))
                    {
                        info.Invoke(target, null);
                    }
                }
            }
        }
    }
}
