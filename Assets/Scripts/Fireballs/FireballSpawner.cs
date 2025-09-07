using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FireballSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject[] fireballPrefabs;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(1f, 3f);

    [Header("References")]
    [SerializeField] private Transform earthTransform;

    [Header("Pooler")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPool = 50;

    [Header("Events")]
    public GameObjectEventChannelSO eventChannel;

    private Dictionary<GameObject, IObjectPool<GameObject>> prefabPools = new();
    private Dictionary<GameObject, IObjectPool<GameObject>> instanceToPool = new();
    private float nextSpawnTime;

    private void OnEnable() => eventChannel.OnEventRaised += ReleasePoolObject;
    private void OnDisable() => eventChannel.OnEventRaised -= ReleasePoolObject;

    private void Awake()
    {
        foreach (var prefab in fireballPrefabs)
        {
            var pool = new ObjectPool<GameObject>(
                () =>
                {
                    var obj = Instantiate(prefab);
                    obj.SetActive(false);
                    return obj;
                },
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj => Destroy(obj),
                false,
                defaultCapacity,
                maxPool
            );
            prefabPools[prefab] = pool;
        }
    }

    private void Start() => ScheduleNextSpawn();

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomFireball();
            ScheduleNextSpawn();
        }
    }

    private void ReleasePoolObject(GameObject obj)
    {
        if (obj != null && instanceToPool.TryGetValue(obj, out var pool))
        {
            pool.Release(obj);
            instanceToPool.Remove(obj);
        }
    }

    private void ScheduleNextSpawn()
    {
        float interval = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
        nextSpawnTime = Time.time + interval;
    }

    private void SpawnRandomFireball()
    {
        if (fireballPrefabs.Length == 0 || earthTransform == null) return;

        // --- random point on circle circumference (XY plane) ---
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnPos = earthTransform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;

        // Target is earth (optionally with a tiny random offset for variation)
        Vector3 targetPos = earthTransform.position + (Vector3)Random.insideUnitCircle * 1.5f;

        // Direction toward target
        Vector3 dir = (targetPos - spawnPos).normalized;
        float zRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Get fireball from pool
        GameObject prefab = fireballPrefabs[Random.Range(0, fireballPrefabs.Length)];
        var pool = prefabPools[prefab];
        GameObject fireball = pool.Get();
        instanceToPool[fireball] = pool;

        // Place + rotate so that transform.up points inward
        fireball.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0f, 0f, zRot - 90f));

        // Initialize movement
        fireball.GetComponent<IFireball>().Initialize(targetPos, pool);
    }
}
