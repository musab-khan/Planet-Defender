using UnityEngine;

public interface IFireball
{
    void Initialize(Vector3 targetPosition);
    void OnEarthImpact();
    int GetScore();

    FireballType GetFireballType();
}

public enum FireballType
{
    Normal
}