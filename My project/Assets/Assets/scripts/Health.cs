using UnityEngine;

public class Health : MonoBehaviour, Damageable
{
    public float maxHP = 100f;
    public GameObject deathVFX;   // optional

    float hp;

    void Awake() => hp = maxHP;

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        hp -= amount;
        if (hp <= 0f) Die(hitPoint, hitNormal);
    }

    void Die(Vector3 p, Vector3 n)
    {
        if (deathVFX)
        {
            var v = Instantiate(deathVFX, p, Quaternion.LookRotation(n));
            Destroy(v, 5f);
        }
        Destroy(gameObject);
    }
}
