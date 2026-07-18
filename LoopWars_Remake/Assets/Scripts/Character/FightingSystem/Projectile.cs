using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    private Character attacker;
    protected virtual BulletScriptableObject bulletScriptableObject { get; set; }

    private Rigidbody2D _rb;
    protected Rigidbody2D rigidbody {  
        get 
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
                    return _rb;
        } 
        set { _rb = value; } 
    }
    private BoxCollider2D _c;
    public BoxCollider2D collider {
        get { 
            if (_c == null) _c = GetComponent<BoxCollider2D>();
            return _c; 
        }
        private set
        {
            _c = value;
        }
    }

    private float lifeTime;
    protected float curCanDamageAttackerTimer = 0.1f;

    private bool despawnOnDisable = false;

    public static event Action<Projectile, BulletScriptableObject> onDestroyedStatic;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        LifeTimeControll();
        CanDamageAttackerTimer();
    }

    private void LifeTimeControll()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            OnLifeTime();
    }

    protected virtual void OnLifeTime()
    {
        DisableProjectile();
    }

    private void CanDamageAttackerTimer()
    {
        if(bulletScriptableObject.canDamageAttacker)
        {
            if(curCanDamageAttackerTimer > 0f)
                curCanDamageAttackerTimer -= Time.deltaTime;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsThereADamagable(collision.gameObject, out IDamagable damagable, out bool isAttacker))
        {
            if (isAttacker)
            {
                if (!bulletScriptableObject.canDamageAttacker || curCanDamageAttackerTimer > 0f)
                    return;
            }

            Damage(damagable, bulletScriptableObject.damage, bulletScriptableObject.maxDamageMultiplier);
        }
        else if(IsThereAProjectile(collision.gameObject, out Projectile projectile, out bool isAttackers))
        {
            if (isAttackers && !bulletScriptableObject.canBeDestroyedByAttackersProjectiles)
                return;
        }

        if(bulletScriptableObject.destroyOnImpact)
            DestroyOnImpact();
    }

    protected bool IsThereADamagable(GameObject gameObject, out IDamagable damagable, out bool isAttacker)
    {
        damagable = gameObject.GetComponentInChildren<IDamagable>();

        if (damagable != null)
        {
            HealthSystem healthSystem = damagable as HealthSystem;
            isAttacker = attacker != null && healthSystem != null && healthSystem == attacker.healthSystem;
            return true;
        }

        isAttacker = false;
        return false;
    }

    protected bool IsThereAProjectile(GameObject gameObject, out Projectile projectile, out bool isAttackers)
    {
        projectile = gameObject.GetComponentInChildren<Projectile>();

        if (projectile != null)
        {
            isAttackers = attacker != null && projectile.attacker != null && projectile.attacker == attacker;
            return true;
        }

        isAttackers = false;
        return false;
    }

    protected virtual void Damage(IDamagable damagable, float damage, float maxDamageMultiplier = 1f)
    {
        damage = damage * UnityEngine.Random.Range(1f, maxDamageMultiplier);
        damagable.Damage(attacker.transform, transform, damage);
    }

    protected virtual void DestroyOnImpact()
    {
        onDestroyedStatic?.Invoke(this, bulletScriptableObject);
        DisableProjectile();
    }

    private void DisableProjectile()
    {
        if (despawnOnDisable)
            DespawnProjectile();
        else
            DisableProjectileRpc();
    }    

    private void DisableProjectileRpc()
    {
        if (gameObject == null) return;
        gameObject.SetActive(false);
    }

    private void DespawnProjectile()
    {
        Destroy(gameObject);
    }

    public void LaunchProjectile(Vector2 position, Vector2 direction)
    {
        gameObject.SetActive(true);

        transform.position = position;
        transform.right = direction;

        rigidbody.linearVelocity = direction * bulletScriptableObject.speed;
        lifeTime = bulletScriptableObject.maxLifeTime;

        curCanDamageAttackerTimer = 0.1f;
    }

    private void OnWeaponDespawned(Weapon weapon)
    {
        weapon.onWeaponDespawned -= OnWeaponDespawned;
        if (!gameObject.active)
            DespawnProjectile();
        else
            despawnOnDisable = true;
    }



    public static Projectile CreateNewProjectile(Weapon weapon, BulletScriptableObject bulletScriptableObject, Character attacker, bool despawnOnDestroy)
    {
        Projectile projectile = SpawnProjectile(bulletScriptableObject, attacker, despawnOnDestroy);
        weapon.onWeaponDespawned += projectile.OnWeaponDespawned;

        return projectile;
    }

    public static Projectile CreateNewProjectile(BulletScriptableObject bulletScriptableObject, Character attacker, bool despawnOnDestroy)
    {
        Projectile projectile = SpawnProjectile(bulletScriptableObject, attacker, despawnOnDestroy);

        return projectile;
    }

    private static Projectile SpawnProjectile(BulletScriptableObject bulletScriptableObject, Character attacker, bool despawnOnDestroy)
    {
        Projectile projectile = Instantiate(bulletScriptableObject.bulletPrefab).GetComponent<Projectile>();

        projectile.bulletScriptableObject = bulletScriptableObject;

        projectile.attacker = attacker;
        projectile.despawnOnDisable = despawnOnDestroy;

        return projectile;
    }
}
