using System.Collections.Generic;
using UnityEngine;

public class ChainFollow : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 180f; // degrees per second

    [Header("Chain Settings")]
    public GameObject chainSegmentPrefab;
    public int segmentCount = 10;
    public float segmentDistance = 1f;
    public float followStrength = 10f;
    public float damping = 0.8f;

    [Header("Physics Settings")]
    public bool usePhysicsForAllSegments = false;
    public float chainMass = 1f;
    public float chainDrag = 1f;

    private float turnInput; // -1 = left, +1 = right
    private List<Transform> chainSegments = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private List<Rigidbody> chainRigidbodies = new List<Rigidbody>();

    // Use the anchor's rigidbody for movement
    private Rigidbody anchorRb;

    void Start()
    {
        // Get or add rigidbody to the anchor (this transform)
        anchorRb = GetComponent<Rigidbody>();
        if (anchorRb == null)
        {
            anchorRb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure anchor rigidbody
        anchorRb.mass = chainMass;
        anchorRb.drag = chainDrag;
        anchorRb.freezeRotation = false; // Allow rotation for turning

        CreateChain();
    }

    void CreateChain()
    {
        // Create chain segments
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = Instantiate(chainSegmentPrefab);
            segment.name = $"Chain_Segment_{i}";
            segment.transform.position = transform.position - transform.up * ((i + 1) * segmentDistance);

            chainSegments.Add(segment.transform);
            targetPositions.Add(segment.transform.position);

            // Add rigidbody to segments if using physics for all
            if (usePhysicsForAllSegments)
            {
                Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
                if (segmentRb == null)
                {
                    segmentRb = segment.AddComponent<Rigidbody>();
                }

                segmentRb.mass = chainMass * 0.5f; // Lighter than anchor
                segmentRb.drag = chainDrag;
                chainRigidbodies.Add(segmentRb);
            }
            else
            {
                chainRigidbodies.Add(null);
            }
        }
    }

    private void FixedUpdate()
    {
        MoveAnchor();

        if (usePhysicsForAllSegments)
        {
            UpdateChainWithPhysics();
        }
    }

    void Update()
    {
        if (chainSegments.Count == 0) return;
        UpdateTargetPositions();
    }

    private void MoveAnchor()
    {
        if (anchorRb == null) return;

        // Move forward in local UP direction using forces
        Vector3 forward = transform.up;
        Vector3 targetVelocity = forward * moveSpeed;

        // Apply force to reach target velocity
        Vector3 velocityDiff = targetVelocity - anchorRb.velocity;
        anchorRb.AddForce(velocityDiff * followStrength, ForceMode.Acceleration);

        // Apply rotation
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float torque = -turnInput * turnSpeed;
            anchorRb.AddTorque(0, 0, torque, ForceMode.Acceleration);
        }
    }

    private void LateUpdate()
    {
        if (chainSegments.Count == 0) return;

        if (!usePhysicsForAllSegments)
        {
            UpdateChainWithoutPhysics();
        }
    }

    void UpdateTargetPositions()
    {
        // First segment follows this transform with proper distance
        if (chainSegments.Count > 0)
        {
            Vector3 directionToAnchor = (transform.position - chainSegments[0].position).normalized;
            targetPositions[0] = transform.position - directionToAnchor * segmentDistance;
        }

        // Each subsequent segment follows the previous one
        for (int i = 1; i < chainSegments.Count; i++)
        {
            Vector3 directionToPrevious = (targetPositions[i - 1] - chainSegments[i].position).normalized;
            targetPositions[i] = targetPositions[i - 1] - directionToPrevious * segmentDistance;
        }
    }

    void UpdateChainWithoutPhysics()
    {
        for (int i = 0; i < chainSegments.Count; i++)
        {
            Vector3 targetPos = targetPositions[i];
            Vector3 currentPos = chainSegments[i].position;

            // Smooth movement towards target
            Vector3 newPos = Vector3.Lerp(currentPos, targetPos, followStrength * Time.deltaTime);
            chainSegments[i].position = newPos;

            // Handle rotation
            UpdateSegmentRotation(i);
        }
    }

    void UpdateChainWithPhysics()
    {
        for (int i = 0; i < chainSegments.Count; i++)
        {
            if (chainRigidbodies[i] == null) continue;

            Vector3 targetPos = targetPositions[i];
            Vector3 currentPos = chainSegments[i].position;

            // Apply force towards target position
            Vector3 forceDirection = (targetPos - currentPos);
            float distance = forceDirection.magnitude;

            if (distance > 0.1f) // Only apply force if far enough
            {
                Vector3 force = forceDirection.normalized * followStrength * distance;
                chainRigidbodies[i].AddForce(force, ForceMode.Acceleration);
            }

            // Apply damping
            chainRigidbodies[i].velocity *= damping;

            // Handle rotation
            UpdateSegmentRotation(i);
        }
    }

    void UpdateSegmentRotation(int segmentIndex)
    {
        Vector3 direction = Vector3.zero;

        if (segmentIndex == 0)
        {
            // First segment looks towards the anchor
            direction = (transform.position - chainSegments[segmentIndex].position).normalized;
        }
        else
        {
            // Other segments look towards the previous segment
            direction = (chainSegments[segmentIndex - 1].position - chainSegments[segmentIndex].position).normalized;
        }

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

            if (usePhysicsForAllSegments && chainRigidbodies[segmentIndex] != null)
            {
                // Use physics rotation
                chainRigidbodies[segmentIndex].MoveRotation(
                    Quaternion.Slerp(chainSegments[segmentIndex].rotation, targetRotation, followStrength * Time.fixedDeltaTime)
                );
            }
            else
            {
                // Direct rotation
                chainSegments[segmentIndex].rotation = Quaternion.Slerp(
                    chainSegments[segmentIndex].rotation,
                    targetRotation,
                    followStrength * Time.deltaTime
                );
            }
        }
    }

    public void SetDirection(Vector2 input)
    {
        turnInput = input.x;
    }

    // Public methods for runtime modification
    public void AddSegment()
    {
        if (chainSegments.Count > 0)
        {
            GameObject segment = Instantiate(chainSegmentPrefab);
            segment.name = $"Chain_Segment_{chainSegments.Count}";
            Transform lastSegment = chainSegments[chainSegments.Count - 1];
            segment.transform.position = lastSegment.position - lastSegment.up * segmentDistance;

            chainSegments.Add(segment.transform);
            targetPositions.Add(segment.transform.position);

            // Handle rigidbody for new segment
            if (usePhysicsForAllSegments)
            {
                Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
                if (segmentRb == null)
                {
                    segmentRb = segment.AddComponent<Rigidbody>();
                }
                segmentRb.mass = chainMass * 0.5f;
                segmentRb.drag = chainDrag;
                chainRigidbodies.Add(segmentRb);
            }
            else
            {
                chainRigidbodies.Add(null);
            }
        }
    }

    public void RemoveSegment()
    {
        if (chainSegments.Count > 0)
        {
            int lastIndex = chainSegments.Count - 1;
            if (chainSegments[lastIndex] != null)
            {
                DestroyImmediate(chainSegments[lastIndex].gameObject);
            }
            chainSegments.RemoveAt(lastIndex);
            targetPositions.RemoveAt(lastIndex);
            chainRigidbodies.RemoveAt(lastIndex);
        }
    }

    // Get chain segments for enemy detection
    public List<Transform> GetChainSegments()
    {
        return chainSegments;
    }
}