using UnityEngine;
using UnityEngine.Pool;


// Base fireball class that's open for extension, closed for modification
public abstract class BaseFireball : MonoBehaviour, IFireball
{
    [SerializeField] private FireballData data;
    [SerializeField] private GameObject nullifiedEffect;
    [SerializeField] private GameObject impactEffect;

    private IObjectPool<GameObject> pool;

    protected Rigidbody rb;
    protected bool isNullified = false;
    protected Vector3 targetPosition;
    private float checkDelay = 0f;

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

    public virtual void Initialize(Vector3 target, IObjectPool<GameObject> pool)
    {
        this.pool = pool;
        targetPosition = target;
        rb.velocity = transform.up * data.fallSpeed;
    }

    public virtual void OnEarthImpact()
    {
        if (isNullified) return;

        // Visual effect for impact
        CreateImpactEffect();
        pool.Release(gameObject);
    }

    private void OnEncapsulated()
    {
        if (isNullified) return;

        isNullified = true;
        CreateNullificationEffect();

        pool.Release(gameObject);
    }

    private void Update()
    {
        checkDelay += Time.deltaTime;
        if (checkDelay >= data.detectionInterval)
        {
            GetNearbySnakeSegments();
            checkDelay = 0f;
        }
    }

    public virtual int GetScore() => data.scoreValue;

    protected virtual void CreateImpactEffect()
    {
        data.particleSpawn.RaiseEvent(impactEffect, transform.position, Quaternion.identity);
    }

    protected virtual void CreateNullificationEffect()
    {
        data.particleSpawn.RaiseEvent(nullifiedEffect, transform.position, Quaternion.identity);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Earth"))
        {
            OnEarthImpact();
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damage))
            {
                damage.TakeDamage(data.earthDamage);
            }
        }
    }

    private int surroundCount = 0;
    public void GetNearbySnakeSegments()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, data.detectionRadius, data.snakeLayerMask);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Segment"))
            {
                if (collider.GetComponent<IEncapsulatable>().CheckEncapsulation(transform))
                {
                    surroundCount++;

                    if (surroundCount >= data.minEncapsulation)
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
