using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public Character attacker;

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
    [HideInInspector] public bool destroyOnImpact;

    [HideInInspector] public float lifeTime;
    private bool canDamageAttacker = false;

    private void Update()
    {
        LifeTimeControll();
    }

    private void LifeTimeControll()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            DestroyProjectile();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsThereADamagable(collision.gameObject, out IDamagable damagable, out bool isAttacker))
        {
            if(isAttacker && !canDamageAttacker)
            {
                canDamageAttacker = true;
                return;
            }

            float damage = this.damage * Random.Range(1f, maxDamageMultiplier);
            damagable.Damage(attacker.transform, transform, damage);
        }

        if(destroyOnImpact)
            DestroyProjectile();
    }

    private bool IsThereADamagable(GameObject gameObject, out IDamagable damagable, out bool isAttacker)
    {
        damagable = gameObject.GetComponentInChildren<IDamagable>();

        if(damagable != null)
        {
            HealthSystem healthSystem = damagable as HealthSystem;
            isAttacker = attacker != null && healthSystem != null && healthSystem == attacker.healthSystem;
            return true;
        }

        isAttacker = false;
        return false;
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }





    public static Projectile CreateNewProjectile(BulletScriptableObject bulletScriptableObject, Character attacker, Vector2 position, Vector2 direction)
    {
        Projectile projectile = Instantiate(bulletScriptableObject.bulletPrefab).GetComponent<Projectile>();

        projectile.transform.position = position;
        projectile.transform.right = direction;

        projectile.rigidbody.linearVelocity = direction * bulletScriptableObject.speed;

        projectile.damage = bulletScriptableObject.damage;
        projectile.maxDamageMultiplier = bulletScriptableObject.maxDamageMultiplier;
        projectile.destroyOnImpact = bulletScriptableObject.destroyOnImpact;
        projectile.lifeTime = bulletScriptableObject.maxLifeTime;
        projectile.attacker = attacker;

        return projectile;

    }
}
