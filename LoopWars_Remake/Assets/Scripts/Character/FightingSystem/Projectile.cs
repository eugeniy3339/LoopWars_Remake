using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public HealthSystem attacker;

    private Rigidbody2D _rb;
    public Rigidbody2D rigidbody {  
        get 
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
                    return _rb;
        } 
        set { _rb = value; } 
    }

    [HideInInspector] public float damage;
    [HideInInspector] public float maxDamageMultiplier;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }





    public static Projectile CreateNewProjectile(BulletScriptableObject bulletScriptableObject, HealthSystem attacker, Vector2 position, Vector2 direction)
    {
        Projectile projectile = Instantiate(bulletScriptableObject.bulletPrefab).GetComponent<Projectile>();

        projectile.transform.position = position;
        projectile.transform.right = direction;

        projectile.rigidbody.linearVelocity = direction * bulletScriptableObject.speed;

        projectile.damage = bulletScriptableObject.damage;
        projectile.maxDamageMultiplier = bulletScriptableObject.maxDamageMultiplier;

        return projectile;

    }
}
