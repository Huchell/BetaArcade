using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable_Line : MonoBehaviour {

    public int coinCount = 2; //should be 1 minimum.
    public GameObject prefab, startButton, endButton;
    [HideInInspector]
    public GameObject[] midButtons;
    public bool DrawLine = true;
    private Vector3 endButtonLastPosition;
    //startButton is the owner of this, and will always exist

    private void OnDrawGizmos()
    {
        if (DrawLine)
        {
            Gizmos.DrawLine(startButton.transform.position, endButton.transform.position);
        }
    }

    public void Start()
    {
        transform.parent.GetComponent<MeshRenderer>().enabled = false;
    }

    public void RefreshButtons()
    {
        if (coinCount < 2)
        {
            if (endButton)
            {
                DestroyImmediate(endButton);
            }
        }
        else
        {
            if (!endButton)
            {
                GameObject coin = Instantiate(prefab);
                coin.transform.SetParent(transform.parent, false);
                coin.transform.localScale = new Vector3(1f / transform.parent.localScale.x, 1f / transform.parent.localScale.y, 1f / transform.parent.localScale.z);
                if (endButtonLastPosition != null && endButtonLastPosition != Vector3.zero)
                {
                    coin.transform.position = endButtonLastPosition;
                }
                else
                {
                    coin.transform.position = startButton.transform.position + new Vector3(1, 0, 0);
                    endButtonLastPosition = coin.transform.position;
                }
                coin.AddComponent<Collectable_LineEnd>().startButton = startButton;
                coin.name = "Collectable_Line_End";
                endButton = coin;
            }
            else
            {
                    endButtonLastPosition = endButton.transform.position;
            }

            for (int x = midButtons.Length - 1; x >= 0; x--)
            {
                DestroyImmediate(midButtons[x]);
            }

            midButtons = new GameObject[coinCount - 2];

            for (int x = 0; x < coinCount-2; x++)
            {
                GameObject coin = Instantiate(prefab);
                midButtons[x] = coin;
                coin.transform.SetParent(transform.parent, false);
                coin.transform.position = Vector3.Lerp(startButton.transform.position, endButton.transform.position, (float)(x+1)/(float)(coinCount - 1));
                coin.transform.localScale = new Vector3(1f/transform.parent.localScale.x, 1f/transform.parent.localScale.y, 1f/transform.parent.localScale.z);
                coin.name = "Collectable_Line_[" + (x+1) + "]";
            }

            //OnDrawGizmos();
        }
    }
}
