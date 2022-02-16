using UnityEngine;

public interface IDamagable
{
    void ApplyDamage(float damage, Vector3 hitPoint ,Vector3 hitDirection);
}