using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/GameObject Spawn Event Channel")]
public class ObjectSpawnEventChannelSO : ScriptableObject
{
    public UnityAction<GameObject, Vector3, Quaternion> OnEventRaised;

    public void RaiseEvent(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        OnEventRaised?.Invoke(prefab, pos, rot);
    }
}
