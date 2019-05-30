using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{

    [System.Serializable]
    public class Pool
    {
        public string id;
        public int size;
        public GameObject prefab;
    }

    public static ObjectPooler instance;
    
    public Pool[] pools;
    
    private Dictionary<string, Queue<GameObject>> objectPools;

    void Awake()
    {
        if (ObjectPooler.instance == null)
            instance = this;

        else
            Destroy(this);
    }

    void Start()
    {
        objectPools = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; ++i)
            {
                GameObject go = Instantiate(pool.prefab);
                go.SetActive(false);
                
                objectPool.Enqueue(go);
            }

            objectPools.Add(pool.id, objectPool);
        }
    }

    public GameObject SpawnFromPool(string id, Vector3 position, Quaternion rotation)
    {

        if (!objectPools.ContainsKey(id))
        {
            Debug.LogWarning("Pool id " + id + " doesn't exist. Attempted spawn of object failed.");
            return null;
        }
        
        GameObject go = objectPools[id].Dequeue();

        go.SetActive(true);
        go.transform.position = position;
        go.transform.rotation = rotation;
        
        go.GetComponent<IPooledObject>()?.OnPooledObjectStart();
        
        objectPools[id].Enqueue(go);

        return go;
    }
}
