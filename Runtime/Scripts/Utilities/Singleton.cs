using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null)
                    instance = FindObjectOfTypeInScene();
            }

            return instance;
        }
    }

    public static bool HasInstance
    {
        get
        {
            return instance != null;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this as T;
    }

    public virtual void OnDestroy()
    {
        Destroy();
    }

    public void Destroy()
    {
        if (instance == this)
            instance = null;
    }
    private static T FindObjectOfTypeInScene()
    {
        var allLoadedGo = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < allLoadedGo.Length; i++)
        {
            var f = allLoadedGo[i].GetComponentInChildren<T>(true);
            if (f != null)
                return f;
        }
        return null;
    }
}