using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base fireball class that's open for extension, closed for modification
public abstract class BaseFireball : MonoBehaviour, IFireball
{
    [Header("Fireball Settings")]
    public int scoreValue = 10;
    public int earthDamage = 10;
    public float fallSpeed = 5f;
    public int minEncapsulation = 3;
    public bool canBeEncapsulated = true;
    public GameObject nullifiedEffect;
    public GameObject impactEffect;

    [Header("Snake Detection Settings")]
    public float detectionRadius = 3f;
    public LayerMask snakeLayerMask = -1; // Which layers to check for snake parts

    protected Rigidbody rb;
    protected bool isNullified = false;
    protected Vector3 targetPosition;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Add bouncy physics material
        PhysicMaterial bouncyMaterial = new PhysicMaterial("Bouncy")
        {
            bounciness = 0.8f,
            dynamicFriction = 0.1f
        };

        GetComponent<Collider>().material = bouncyMaterial;
    }

    public virtual void Initialize(Vector3 target)
    {
        targetPosition = target;
        rb.velocity = transform.up * fallSpeed;
    }

    public virtual void OnEarthImpact()
    {
        if (isNullified) return;

        // Visual effect for impact
        CreateImpactEffect();

        Destroy(gameObject);
    }

    private void OnEncapsulated()
    {
        if (isNullified) return;

        isNullified = true;
        CreateNullificationEffect();
        Destroy(gameObject);
    }

    public virtual int GetScore() => scoreValue;

    protected virtual void CreateImpactEffect()
    {
       
    }

    protected virtual void CreateNullificationEffect()
    {
        
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Earth"))
        {
            OnEarthImpact();
        }
    }

    private int surroundCount = 0;
    public void GetNearbySnakeSegments()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, snakeLayerMask);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("SnakeSegment"))
            {
                if (collider.GetComponent<IEncapsulatable>().CheckEncapsulation(transform))
                {
                    surroundCount++;

                    if (surroundCount >= minEncapsulation)
                    {
                        OnEncapsulated();
                    }
                }
            }
        }

        surroundCount = 0;
    }

    public abstract FireballType GetFireballType();
}
