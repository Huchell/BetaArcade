using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooksComeAndGoLogic : MonoBehaviour {
    
    [System.Serializable]
    public class books
    {
        public GameObject prefab;
        public bool activeWhileStoodOn = true;
    }

    public List<books> bookList;

    [HideInInspector]
    public List<GameObject> booksInWorld = new List<GameObject>();
    public float spawnDelay = 1.5f;

    [HideInInspector]
    public Material ghost;

    // Use this for initialization
    void Start ()
    {
        for (int book = 0; book < booksInWorld.Count; book++)
		{
            if (bookList[book].activeWhileStoodOn)
            {
                booksInWorld[book].GetComponent<MeshRenderer>().sharedMaterial = bookList[book].prefab.GetComponent<MeshRenderer>().sharedMaterial;
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        StartCoroutine(bookDelay(0, false));
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(bookDelay(spawnDelay, true));
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(bookDelay(spawnDelay, false));
    }
    
    public void ResetBooks()
    {
        foreach (GameObject book in booksInWorld)
        {
            DestroyImmediate(book);
        }

        booksInWorld.Clear();

        for (int x = 0; x < bookList.Count; x++)
        {
            if (bookList[x].prefab)
            {
                GameObject newBook = Instantiate(bookList[x].prefab);
                booksInWorld.Add(newBook);
                newBook.name += "[" + x + "]";

                newBook.transform.SetParent(transform, false);

                newBook.transform.position = transform.position + new Vector3(0, 0, 2f*x);
                
                if (bookList[x].activeWhileStoodOn)
                {
                    newBook.GetComponent<MeshRenderer>().sharedMaterial = ghost;
                }
            }
        }
    }

    public void UpdateBooks()
    {
        for (int x = 0; x < bookList.Count; x++)
        {
            if (bookList[x].activeWhileStoodOn)
            {
                booksInWorld[x].GetComponent<MeshRenderer>().sharedMaterial = ghost;
                booksInWorld[x].GetComponent<Collider>().enabled = false;
            }
            else
            {
                booksInWorld[x].GetComponent<MeshRenderer>().sharedMaterial = bookList[x].prefab.GetComponent<MeshRenderer>().sharedMaterial;
                booksInWorld[x].GetComponent<Collider>().enabled = true;
            }
        }
    }

    IEnumerator bookDelay(float delay, bool show)
    {
        yield return new WaitForSeconds(delay);

        if (show)
        {
            for (int book = 0; book < booksInWorld.Count; book++)
            {
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = bookList[book].activeWhileStoodOn;
                booksInWorld[book].GetComponent<Collider>().enabled = bookList[book].activeWhileStoodOn;
            }
        }
        else
        {
            for (int book = 0; book < booksInWorld.Count; book++)
            {
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = !bookList[book].activeWhileStoodOn;
                booksInWorld[book].GetComponent<Collider>().enabled = !bookList[book].activeWhileStoodOn;
            }
        }
    }

}
