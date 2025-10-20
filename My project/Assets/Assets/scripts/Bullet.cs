using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float damage = 25f;
    public float maxLife = 3f;

    Rigidbody rb;
    float deathAt;
    bool alreadyHit;

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
        if (alreadyHit) return;
        alreadyHit = true;

        rb.linearVelocity = Vector3.zero;

        var dmg = c.collider.GetComponentInParent<Damageable>();
        if (dmg != null)
        {
            var cp = c.GetContact(0);
            dmg.TakeDamage(damage, cp.point, cp.normal);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Bullet] Trigger hit: {other.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        // check if the thing hit has a Damageable component in its parents
        var dmg = other.GetComponentInParent<Damageable>();
        Debug.Log($"[Bullet] Found Damageable? {(dmg != null ? "YES" : "NO")}");

        if (dmg != null)
        {
            // stop the bullet and apply damage
            if (rb) rb.linearVelocity = Vector3.zero;

            Vector3 dir = rb && rb.linearVelocity.sqrMagnitude > 0.001f
                ? rb.linearVelocity.normalized
                : transform.forward;

            dmg.TakeDamage(damage, transform.position, -dir);
        }

        Destroy(gameObject);
    }


}
