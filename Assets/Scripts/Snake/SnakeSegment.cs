using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : MonoBehaviour, IEncapsulatable
{
    [SerializeField] private float encapsulateRadius;
    public bool CanBeEncapsulate()
    {
        throw new System.NotImplementedException();
    }

    public bool CheckEncapsulation(Transform obj)
    {
        if (Vector3.Distance(transform.position, obj.position) <= encapsulateRadius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
