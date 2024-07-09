using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public bool showUninitiatedPoolWarning = false;

    public List<Pool> poolList = new List<Pool>();
    public static Pool GetPool(GameObject go)
    {
        int ID = Instance.GetPoolID(go);
        if (ID == -1) 
            return null;
        return Instance.poolList[ID];
    }
    public static void ClearAll()
    {   //not in used
        for (int i = 0; i < Instance.poolList.Count; i++) Instance.poolList[i].Clear();
        Instance.poolList = new List<Pool>();
    }

    public static Transform GetOPMTransform() { return Instance.transform; }

    public static Transform Spawn(Transform objT, float activeDuration = -1)
    {
        return Spawn(objT.gameObject, Vector3.zero, Quaternion.identity, activeDuration).transform;
    }
    public static Transform Spawn(Transform objT, Vector3 pos, Quaternion rot, float activeDuration = -1)
    {
        return Instance._Spawn(objT.gameObject, pos, rot, activeDuration).transform;
    }

    public static GameObject Spawn(GameObject obj, float activeDuration = -1)
    {
        return Spawn(obj, Vector3.zero, Quaternion.identity, activeDuration);
    }
    public static GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration = -1)
    {
        return Instance._Spawn(obj, pos, rot, activeDuration);
    }
    public GameObject _Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration = -1)
    {
        if (obj == null)
        {
            Debug.LogWarning("NullReferenceException: obj unspecified");
            return null;
        }

        int ID = GetPoolID(obj);

        if (ID == -1)
        {
            if (showUninitiatedPoolWarning)
                Debug.LogWarning("ObjectPoolManager: trying to spawn uninitiated object (" + obj + "), creating new pool");
            ID = _New(obj);
        }

        GameObject spawnedObj = poolList[ID].Spawn(pos, rot);

        if (activeDuration > 0) StartCoroutine(UnspawnRoutine(spawnedObj, activeDuration));

        return spawnedObj;
    }

    IEnumerator UnspawnRoutine(GameObject spawnedObj, float activeDuration)
    {
        yield return new WaitForSeconds(activeDuration);
        Unspawn(spawnedObj);
    }

    public static void Unspawn(Transform objT, float delay)
    {
        Instance.StartCoroutine(Instance.UnspawnRoutine(objT.gameObject, delay));
    }
    private static Dictionary<GameObject, Coroutine> unspawningQueue = new Dictionary<GameObject, Coroutine>();
    public static void Unspawn(GameObject obj, float delay)
    {
        if(obj != null)
            unspawningQueue[obj] = Instance.StartCoroutine(Instance.UnspawnRoutine(obj, delay));
    }

    public static void Unspawn(Transform objT) { Instance._Unspawn(objT.gameObject); }
    public static void Unspawn(GameObject obj) { Instance._Unspawn(obj); }
    public void _Unspawn(GameObject obj)
    {
        if (obj == null) return;

        if (unspawningQueue.TryGetValue(obj, out Coroutine co))
        {
            if(co != null)
                StopCoroutine(co);
        }
        unspawningQueue[obj] = null;

        for (int i = 0; i < poolList.Count; i++)
        {
            if (poolList[i].Unspawn(obj)) return;
        }
        Destroy(obj);
    }

    public static int New(Transform objT, int count = 1) { return Instance._New(objT.gameObject, count); }
    public static int New(GameObject obj, int count = 1) 
    {
        return Instance._New(obj, count); 
    }
    public int _New(GameObject obj, int count = 1)
    {
        int ID = GetPoolID(obj);

        if (ID != -1) poolList[ID].MatchObjectCount(count);
        else
        {
            Pool pool = new Pool();
            pool.prefab = obj;
            pool.MatchObjectCount(count);
            poolList.Add(pool);
            ID = poolList.Count - 1;
        }

        return ID;
    }

    int GetPoolID(GameObject obj)
    {
        for (int i = 0; i < poolList.Count; i++)
        {
            if (poolList[i].prefab == obj) return i;
        }
        return -1;
    }
}

[System.Serializable]
public class Pool
{
    public GameObject prefab;

    public List<GameObject> inactiveList = new List<GameObject>();
    public List<GameObject> activeList = new List<GameObject>();

    public int cap = 1000;


    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        GameObject obj = null;
        if (inactiveList.Count == 0)
        {
            obj = (GameObject)MonoBehaviour.Instantiate(prefab, pos, rot);
        }
        else
        {
            obj = inactiveList[0];
            obj.transform.parent = null;
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.SetActive(true);
            inactiveList.RemoveAt(0);
        }
        activeList.Add(obj);
        return obj;
    }

    public bool Unspawn(GameObject obj)
    {
        if (obj == null)
            return true;
        if (activeList.Contains(obj))
        {
            obj.SetActive(false);
            obj.transform.parent = ObjectPoolManager.GetOPMTransform();
            activeList.Remove(obj);
            inactiveList.Add(obj);
            return true;
        }
        if (inactiveList.Contains(obj))
        {
            return true;
        }
        return false;
    }

    public void MatchObjectCount(int count)
    {
        if (count > cap) return;
        int currentCount = GetTotalObjectCount();
        for (int i = currentCount; i < count; i++)
        {
            GameObject obj = (GameObject)MonoBehaviour.Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.parent = ObjectPoolManager.GetOPMTransform();
            inactiveList.Add(obj);
        }
    }

    public int GetTotalObjectCount()
    {
        return inactiveList.Count + activeList.Count;
    }

    public void Clear()
    {
        for (int i = 0; i < inactiveList.Count; i++)
        {
            if (inactiveList[i] != null) MonoBehaviour.Destroy(inactiveList[i]);
        }
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i] != null) MonoBehaviour.Destroy(inactiveList[i]);
        }
    }
}

