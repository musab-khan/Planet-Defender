using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : IEncapsulatable
{
    [SerializeField] private float encapsulateRadius;
    public bool CanBeEncapsulate()
    {
        throw new System.NotImplementedException();
    }

    public bool CheckEncapsulation(Transform obj)
    {
        return true;
    }
}
