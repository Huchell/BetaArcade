using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BookStack))]
public class BookStackEditor : Editor {

    SerializedProperty
        sp_BookPrefabs;

    BookStack bs_Target;

    private void OnEnable()
    {
        sp_BookPrefabs = serializedObject.FindProperty("m_BookPrefabs");
        bs_Target = target as BookStack;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        for (int i = 0; i < sp_BookPrefabs.arraySize; i++)
        {
            if (GUILayout.Button(string.Format("Add {0}", sp_BookPrefabs.GetArrayElementAtIndex(i).objectReferenceValue.name)))
            {
                bs_Target.AddBook(i);
            }
        }
    }
}
