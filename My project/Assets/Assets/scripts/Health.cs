using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class Health : MonoBehaviour, Damageable
{
    public float maxHP = 100f;
    float currentHP;

    [Header("Optional UI")]
    public Slider healthbar; // assign a world-space slider if you want a visible bar

    void Awake()
    {
        currentHP = maxHP;
        if (healthbar)
        {
            healthbar.minValue = 0f;
            healthbar.maxValue = maxHP;
            healthbar.value = maxHP;
        }
    }

    // This is required by your Damageable interface
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log($"[Health:{name}] TakeDamage {amount}");
        currentHP = Mathf.Max(0f, currentHP - amount);

        if (healthbar)
            healthbar.value = currentHP;

        if (currentHP <= 0f)
        {
            // Optional: play death effect here
            Destroy(gameObject);
        }
    }
}
