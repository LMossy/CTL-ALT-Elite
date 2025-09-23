using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHP = 100f;
    float hp;

    void Awake() => hp = maxHP;

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f) Die();
    }

    void Die()
    {
        // TODO: death VFX, ragdoll, disable AI, etc.
        Destroy(gameObject);
    }
}
