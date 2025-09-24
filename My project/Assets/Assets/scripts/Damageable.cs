using UnityEngine;

public interface Damageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}
