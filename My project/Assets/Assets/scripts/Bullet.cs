using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float damage = 25f;
    public float maxLife = 3f;

    Rigidbody rb;
    float deathAt;

    public void Init(Vector3 velocity)
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearVelocity = velocity;   // Unity 6
        deathAt = Time.time + maxLife;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.time >= deathAt) Destroy(gameObject);

        // keep forward aligned with velocity for nicer trails
        if (rb && rb.linearVelocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
    }

    void OnCollisionEnter(Collision c)
    {
        // stop bullet
        rb.linearVelocity = Vector3.zero;

        // Damage if target implements Damageable (e.g., Health)
        var dmg = c.collider.GetComponentInParent<Damageable>();
        if (dmg != null)
        {
            var cp = c.GetContact(0);
            dmg.TakeDamage(damage, cp.point, cp.normal);
        }

        Destroy(gameObject);
    }
}
