using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector3Event : UnityEvent<Vector3> { }
[RequireComponent(typeof(Rigidbody))]
public class SnakeHead : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 180f; // degrees per second

    public GameObject segmentPrefab;
    public int initialSegments = 5;
    public float segmentSpacing = 0.5f;

    private LinkedList<Transform> segments = new LinkedList<Transform>();

    private float turnInput; // -1 = left, +1 = right
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ   // stay in XY plane
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY;  // only rotate around Z
    }

    private void Start()
    {
        segments.AddFirst(transform);

        for (int i = 0; i < initialSegments; i++)
            AddSegment();
    }

    void FixedUpdate()
    {
        MoveHead();
        MoveSegments();
    }

    private void MoveHead()
    {
        // Move forward in local UP direction
        Vector3 forward = transform.up;
        rb.velocity = forward * moveSpeed;

        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float rotation = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, rotation));
        }
    }

    private void MoveSegments()
    {
        // Start from the 2nd node (first is head)
        var node = segments.First.Next;

        while (node != null)
        {
            Transform segment = node.Value;
            Transform prev = node.Previous.Value;

            float dist = Vector3.Distance(segment.position, prev.position);
            if (dist > segmentSpacing)
            {
                Vector3 dir = (prev.position - segment.position).normalized;
                segment.position += dir * (dist - segmentSpacing);
                segment.rotation = Quaternion.LookRotation(Vector3.forward, dir);
            }

            node = node.Next; // move along the chain
        }
    }

    public void SetDirection(Vector2 input)
    {
        turnInput = input.x;
    }

    public void AddSegment()
    {
        Transform last = segments.Last.Value;
        Vector3 spawnPos = last.position - last.up * segmentSpacing;
        GameObject seg = Instantiate(segmentPrefab, spawnPos, last.rotation, transform);
        segments.AddLast(seg.transform);
    }
}
