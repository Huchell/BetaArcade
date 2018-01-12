using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBossEnterance : MonoBehaviour {

	public void Check()
    {
        BossEnterance e = FindObjectOfType<BossEnterance>();
        if (e)
        {
            e.Check();
        }
    }
}
