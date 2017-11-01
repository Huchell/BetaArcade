using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class SaveBox : MonoBehaviour
{
    //SaveLoad saveFile;
    [SerializeField]
    public static Vector3 playerPosition;

    public void Start()
    {
        Debug.Log("START");
        Debug.Log(Load()); //DO NOT DO THIS HERE, otherwise the game will attempt to load position for EVERY existing savebox.
        //Load would be setting player.transform.position = SaveLoad.Load();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("Save attempt...");
            Save(collision.gameObject.transform.position);
        }
    }
    
    public static void Save(Vector3 pos)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (!File.Exists(Application.persistentDataPath + "/savedPosition.dat"))
        {
            file = File.Create(Application.persistentDataPath + "/savedPosition.dat");
        }
        file = File.Open(Application.persistentDataPath + "/savedPosition.dat", FileMode.Open);
        SaveLoad data = new SaveLoad();
        data.playerPositionX = pos.x;
        data.playerPositionY = pos.y;
        data.playerPositionZ = pos.z;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Saved: " + data.playerPositionX + "," + data.playerPositionY + "," + data.playerPositionZ);
    }
    
    public static Vector3 Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedPosition.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedPosition.dat", FileMode.Open);
            SaveLoad data = (SaveLoad)bf.Deserialize(file);
            file.Close();
            return new Vector3(data.playerPositionX, data.playerPositionY, data.playerPositionZ);
        }
        Debug.Log("No value loaded");
        return Vector3.zero;
    }
}

[System.Serializable]
class SaveLoad
{
    [SerializeField]
    public float playerPositionX, playerPositionY, playerPositionZ;
}