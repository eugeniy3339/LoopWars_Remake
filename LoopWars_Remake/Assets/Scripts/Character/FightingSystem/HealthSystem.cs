using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamagable
{
    [SerializeField] private float maxHealth;
    private float health;

    private void Awake()
    {
        health = maxHealth;
    }

    public bool Damage(Transform damager, Transform damageObject, float damage)
    {
        health -= damage;

        if (health <= 0f)
            Die();

        return true;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
