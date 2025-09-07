using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleEffectSpawner : MonoBehaviour
{
    [Header("Particle Prefabs")]
    [SerializeField] private GameObject[] particlePrefabs;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPool = 50;

    [Header("Events")]
    [SerializeField] private ObjectSpawnEventChannelSO particleSpawnEvent;

    private Dictionary<GameObject, IObjectPool<GameObject>> pools = new();

    private void OnEnable()
    {
        particleSpawnEvent.OnEventRaised += Spawn;
    }

    private void OnDisable()
    {
        particleSpawnEvent.OnEventRaised -= Spawn;
    }

    private void Awake()
    {
        foreach (var prefab in particlePrefabs)
        {
            pools[prefab] = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab, transform);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPool
            );
        }
    }

    public void Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab))
        {
            Instantiate(prefab, position, rotation);
            return;
        }

        GameObject particle = pools[prefab].Get();
        particle.transform.SetPositionAndRotation(position, rotation);
        particle.GetComponent<ParticleEffect>().SetPool(pools[prefab]);
        particle.GetComponent<ParticleEffect>().Play();
    }

    private void ReleasePoolObject(GameObject obj)
    {
        pools[obj].Release(obj);
    }
}
