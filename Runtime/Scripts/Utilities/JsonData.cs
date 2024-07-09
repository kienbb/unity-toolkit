using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JSonData
{
    public static void SavingData<T>(string nameFile, T input)
    {
        string dataPath = Application.dataPath + "/ConfigFile/" + nameFile + ".json";
        string potion = JsonUtility.ToJson(input);

        LogSystem.LogSuccess(potion);
        System.IO.File.WriteAllText(dataPath, potion);

        LogSystem.LogSuccess("Saving data to " + dataPath + " done!");
    }

    public static T ReadData<T>(string dataPath)
    {
        string jsonText = System.IO.File.ReadAllText(dataPath);
        T data = JsonUtility.FromJson<T>(jsonText);

        return data;
    }

}
