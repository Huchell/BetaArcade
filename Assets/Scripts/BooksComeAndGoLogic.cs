using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooksComeAndGoLogic : MonoBehaviour {
    
    [System.Serializable]
    public class books
    {
        public GameObject prefab;
        public bool activeWhileOn;
    }

    public List<books> bookList;
    [HideInInspector]
    public List<GameObject> booksInWorld = new List<GameObject>();
    public float spawnDelay = 1.5f;

    public Material ghost;

    // Use this for initialization
    void Start ()
    {
        for (int book = 0; book < booksInWorld.Count; book++)
        {
            if (bookList[book].activeWhileOn)
            {
                booksInWorld[book].GetComponent<MeshRenderer>().sharedMaterial = bookList[book].prefab.GetComponent<MeshRenderer>().sharedMaterial;
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = false;
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(bookDelay(spawnDelay, true));
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(bookDelay(spawnDelay, false));
    }
    
    public void RefreshBooks()
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

                if (bookList[x].activeWhileOn)
                {
                    newBook.GetComponent<MeshRenderer>().sharedMaterial = ghost;
                }
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
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = bookList[book].activeWhileOn;
                booksInWorld[book].GetComponent<Collider>().enabled = bookList[book].activeWhileOn;
            }
        }
        else
        {
            for (int book = 0; book < booksInWorld.Count; book++)
            {
                booksInWorld[book].GetComponent<MeshRenderer>().enabled = !bookList[book].activeWhileOn;
                booksInWorld[book].GetComponent<Collider>().enabled = !bookList[book].activeWhileOn;
            }
        }
    }

}
