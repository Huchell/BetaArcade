using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedChoppingBoard : MonoBehaviour {

    [SerializeField]
    Knife[] Knives;

    [SerializeField]
    Knife knifePrefab;

    public void AddKnife(Knife knife)
    {
        Knife[] newArray = new Knife[Knives.Length + 1];
        
        for (int i = 0; i < Knives.Length; i++)
        {
            newArray[i] = Knives[i];
        }

        newArray[newArray.Length - 1] = knife;

        Knives = newArray;
    }

    public void RemoveKnife(Knife knife)
    {
        int removeIndex = System.Array.FindIndex(Knives, k => k == knife);
        RemoveKnife(removeIndex);
    }
    public void RemoveKnife(int index)
    {
        Knife knife = Knives[index];

        Knife[] newArray = new Knife[Knives.Length - 1];
        
        for (int i = 0; i < Knives.Length; i++)
        {
            if (i != index)
            {
                newArray[i] = Knives[i];
            }
        }

        Knives = newArray;

        if (Application.isEditor)
            DestroyImmediate(knife.gameObject);
        else
            Destroy(knife.gameObject);
    }
}
