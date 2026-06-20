using UnityEngine;

public interface IDamagable
{
    bool Damage(Transform damager, Transform damageObject, float damage);
}
