using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayers : MonoBehaviour {

    [SerializeField]
    private GameObject rabbitPosition, catPosition;
    [SerializeField]
    private bool teleportRabbit = true, teleportCat = true;

	public void Teleport()
    {
        if (teleportRabbit) FindObjectOfType<RabbitController>().transform.position = rabbitPosition.transform.position;
        if (teleportCat) FindObjectOfType<CatController>().transform.position = catPosition.transform.position;
    }
}