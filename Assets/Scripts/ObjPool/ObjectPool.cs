using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("[Pool] Tag không tồn tại: " + tag);
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            // Pool cạn → auto-expand, log cảnh báo để tăng prewarm size
            Pool poolConfig = pools.Find(p => p.tag == tag);
            obj = Instantiate(poolConfig.prefab, transform);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[Pool] '{tag}' pool cạn — auto-expand. Tăng prewarm size lên!");
#endif
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // Reset state TRƯỚC khi kích hoạt (OnSpawnFromPool chạy trước OnEnable)
        obj.GetComponent<IPoolable>()?.OnSpawnFromPool();
        obj.SetActive(true);

        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        // Dọn state TRƯỚC khi tắt (OnReturnToPool chạy trước OnDisable)
        obj.GetComponent<IPoolable>()?.OnReturnToPool();
        obj.SetActive(false);

        if (poolDictionary.ContainsKey(tag))
            poolDictionary[tag].Enqueue(obj);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Trả về số object đang chờ trong pool — dùng cho DebugPanel.</summary>
    public int GetIdleCount(string tag)
        => poolDictionary != null && poolDictionary.TryGetValue(tag, out var q) ? q.Count : 0;
#endif
}