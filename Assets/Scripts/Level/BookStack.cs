using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookStack : MonoBehaviour {

    [SerializeField]
    GameObject[] m_BookPrefabs;

    [SerializeField]
    GameObject[] m_BookArray;

    public void AddBook(int index)
    {
        if (m_BookArray == null)
            m_BookArray = new GameObject[0];

        GameObject newGameObject = Instantiate(m_BookPrefabs[index]);
        newGameObject.transform.SetParent(transform, false);
        newGameObject.transform.position = Vector3.zero;

        Mesh newMesh = newGameObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3 newPos = Vector3.zero;

        if (index > 0)
        {
            GameObject lastBookObject = m_BookArray[m_BookArray.Length - 1];
            newPos.y = lastBookObject.transform.localPosition.y + newMesh.bounds.size.y;
        }

        newGameObject.transform.localPosition = newPos;

        GameObject[] newArray = new GameObject[m_BookArray.Length + 1];

        for (int i = 0; i < m_BookArray.Length; i++)
        {
            newArray[i] = m_BookArray[i];
        }

        newArray[newArray.Length - 1] = newGameObject;
        m_BookArray = newArray;
    }
}
