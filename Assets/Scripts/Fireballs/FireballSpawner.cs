using UnityEngine;

public class FireballSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject[] fireballPrefabs;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(1f, 3f); // min/max seconds

    [Header("References")]
    [SerializeField] private Transform earthTransform;

    private float nextSpawnTime;

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomFireball();
            ScheduleNextSpawn();
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

        // Random angle around Earth in XY plane
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Spawn position in XY circle around Earth
        Vector3 spawnPos = new Vector3(
            earthTransform.position.x + Mathf.Cos(randomAngle) * spawnRadius,
            earthTransform.position.y + Mathf.Sin(randomAngle) * spawnRadius,
            earthTransform.position.z
        );

        // Target Earth + offset
        Vector3 targetPos = earthTransform.position + Random.insideUnitSphere * 2f;
        targetPos.z = earthTransform.position.z;

        // Direction vector
        Vector3 dir = (targetPos - spawnPos).normalized;

        // Rotation so local up faces Earth
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);

        // Pick random prefab
        GameObject prefab = fireballPrefabs[Random.Range(0, fireballPrefabs.Length)];
        GameObject fireball = Instantiate(prefab, spawnPos, rotation);

        fireball.GetComponent<IFireball>().Initialize(targetPos);
    }
}
