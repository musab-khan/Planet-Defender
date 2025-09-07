using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Head Movement")]
    public float moveSpeed = 5f;          // constant forward speed
    public float turnSpeed = 180f;        // degrees per second for rotation

    [Header("Body Segments")]
    public GameObject segmentPrefab;
    public int segmentCount = 8;
    public float segmentSpacing = 0.3f;
    public float followSmoothness = 10f;

    [Header("Clamp Settings")]
    public Vector2 minBounds = new Vector2(-8f, -4f);   // bottom-left corner
    public Vector2 maxBounds = new Vector2(8f, 4f);     // top-right corner

    private List<Transform> segments = new List<Transform>();
    private float turnInput;

    void Start()
    {
        // Spawn body segments behind the head
        Vector3 prevPos = transform.position;
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 spawnPos = prevPos - transform.up * (i + 1) * segmentSpacing;
            GameObject seg = Instantiate(segmentPrefab, spawnPos, Quaternion.identity);
            segments.Add(seg.transform);
        }
    }

    void Update()
    {
        HandleHeadMovement();
        HandleBodyFollow();
        ClampHeadPosition();
    }

    void HandleHeadMovement()
    {
        transform.Rotate(Vector3.forward, -turnInput * turnSpeed * Time.deltaTime);

        // Move constantly forward
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    void HandleBodyFollow()
    {
        Vector3 prevPos = transform.position;
        Quaternion prevRot = transform.rotation;

        for (int i = 0; i < segments.Count; i++)
        {
            Transform seg = segments[i];

            // target position behind previous link
            Vector3 targetPos = prevPos - prevRot * Vector3.up * segmentSpacing;

            // smooth follow
            seg.position = Vector3.Lerp(seg.position, targetPos, followSmoothness * Time.deltaTime);

            // rotate smoothly
            seg.rotation = Quaternion.Lerp(seg.rotation, prevRot, followSmoothness * Time.deltaTime);

            prevPos = seg.position;
            prevRot = seg.rotation;
        }
    }

    public void SetDirection(Vector2 input)
    {
        turnInput = input.x;
    }

    void ClampHeadPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}
