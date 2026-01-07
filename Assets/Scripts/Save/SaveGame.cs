using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class SaveGame : MonoBehaviour
{

    public void SaveGameOfScene(Save s)
    {
        BinaryFormatter bf = new BinaryFormatter(); 

        string path = Application.persistentDataPath; 

        FileStream file = File.Create(path + "/saveSceneGame.save"); 
        bf.Serialize(file, s); 
        file.Close();

        Debug.Log("Game Saved");


    }

    public Save LoadGameOfScene()
    {
        BinaryFormatter bf = new BinaryFormatter(); 

        string path = Application.persistentDataPath; 

        FileStream file;

        if (File.Exists(path + "/saveSceneGame.save"))
        {
            file = File.Open(path + "/saveSceneGame.save", FileMode.Open);

            Save save = (Save)bf.Deserialize(file);
            file.Close();
            Debug.Log(path + "/saveSceneGame.save");
            Debug.Log("Game Carregado com o save anterior");
            return save;
        }

        return null;
    }
}
