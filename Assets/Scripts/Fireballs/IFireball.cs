using UnityEngine;
using UnityEngine.Pool;

public interface IFireball
{
    void Initialize(Vector3 targetPosition, IObjectPool<GameObject> pool);
    void OnEarthImpact();
    int GetScore();

    FireballType GetFireballType();
}

public enum FireballType
{
    Normal
}