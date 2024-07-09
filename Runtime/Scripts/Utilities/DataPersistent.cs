using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataPersistent
{
    ///////////////////////////////////PRIVATE FUNCS/////////////////////////////////////
    public static T ReadDataExist<T>(string path, bool saveInPersistentFolder = true) where T : class
    {
        if (saveInPersistentFolder)
            path = $"{Application.persistentDataPath}/{path}";
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            var data = formatter.Deserialize(stream) as T;
            stream.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return null;
    }

    public static T ReadDataExist<T>(byte[] bytes) where T : class
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(bytes);
            var data = formatter.Deserialize(stream) as T;
            stream.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return null;
    }

    public static T ReadDataExist<T>(Stream stream) where T : class
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var data = formatter.Deserialize(stream) as T;
            stream.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return null;
    }
    public static void SaveData<T>(string path, T data, bool saveInPersistentFolder = true)
    {
        DataPersistentSaveSchedule.QueueSave<T>(path, data, saveInPersistentFolder);
        //SaveDataNoWait<T>(path, data, saveInPersistentFolder);
    }

    public static void SaveDataNoWait<T>(string path, T data, bool saveInPersistentFolder = true)
    {
        if (data == null)
        {
            return;
        }
        if (saveInPersistentFolder)
            path = $"{Application.persistentDataPath}/{path}";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        LogSystem.LogSuccess($"Saving {path} success!");
    }

    public static void ClearData(string path, bool saveInPersistentFolder = true)
    {
        if (saveInPersistentFolder)
            path = $"{Application.persistentDataPath}/{path}";
        File.Delete(path);
    }

    public static void SaveTexture2D(string path, Texture2D texture, bool saveInPersistentFolder = true)
    {
        if (saveInPersistentFolder)
            path = $"{Application.persistentDataPath}/{path}";

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        LogSystem.LogSuccess($"Saving {path} success!");
    }

    public static Texture2D ReadTexture2D(string path, int width, int height, bool saveInPersistentFolder = true)
    {
        if (saveInPersistentFolder)
            path = $"{Application.persistentDataPath}/{path}";

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(bytes);

            return texture;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            return null;
        }
    }
}

public class DataPersistentSaveSchedule : MonoBehaviour
{
    private static DataPersistentSaveSchedule _instance;
    private static DataPersistentSaveSchedule instance
    {
        get { if (_instance == null) _instance = new GameObject("DataPersistentSaveSchedule").AddComponent<DataPersistentSaveSchedule>(); return _instance; }
    }
    private float cooldownSave;
    List<Instruction> queueSave = new List<Instruction>();
    public struct Instruction
    {
        public string path;
        public object data;
        public bool saveInPersistent;

        public Instruction(string path, object data, bool saveInPersistent)
        {
            this.path = path;
            this.data = data;
            this.saveInPersistent = saveInPersistent;
        }
    }
    private void FixedUpdate()
    {
        cooldownSave -= Time.deltaTime;
        if (cooldownSave > 0.05f) return;
        cooldownSave = 0.05f;
        if (queueSave.Count > 0)
        {
            for(int i = 0; i < queueSave.Count; i++)
            {
                DataPersistent.SaveDataNoWait(queueSave[i].path, queueSave[i].data, queueSave[i].saveInPersistent);
            }
            queueSave.Clear();
        }
    }

    public static void QueueSave<T>(string path, T data, bool saveInPersistentFolder = true)
    {
        if (instance.queueSave.Exists(x => x.data.Equals(data)))
            return;
        instance.queueSave.Add(new Instruction(path, data, saveInPersistentFolder));
    }
}
