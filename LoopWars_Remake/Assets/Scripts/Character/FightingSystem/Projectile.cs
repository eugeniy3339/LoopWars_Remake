using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : NetworkBehaviour
{
    private Character attacker;
    private BulletScriptableObject bulletScriptableObject;

    private Rigidbody2D _rb;
    private Rigidbody2D rigidbody {  
        get 
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
                    return _rb;
        } 
        set { _rb = value; } 
    }

    private float damage;
    private float maxDamageMultiplier;
    private bool destroyOnImpact;

    private float lifeTime;
    private bool canDamageAttacker = false;

    private bool despawnOnDisable = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameObject.SetActive(false);
        if (!IsServer) { enabled = false; }
    }

    private void Update()
    {
        LifeTimeControll();
    }

    private void LifeTimeControll()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            DisableProjectile();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (IsThereADamagable(collision.gameObject, out IDamagable damagable, out bool isAttacker))
        {
            if (isAttacker && !canDamageAttacker)
            {
                canDamageAttacker = true;
                return;
            }

            Damage(damagable);
        }

        if(destroyOnImpact)
            DisableProjectile();
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

    protected virtual void Damage(IDamagable damagable)
    {
        if (!IsServer) return;
        float damage = this.damage * Random.Range(1f, maxDamageMultiplier);
        damagable.Damage(attacker.transform, transform, damage);
    }

    private void DisableProjectile()
    {
        if (despawnOnDisable)
            DespawnProjectile();
        else
            DisableProjectileRpc();
    }    

    [Rpc(SendTo.Everyone)]
    private void DisableProjectileRpc()
    {
        if (gameObject == null) return;
        gameObject.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    public void LaunchProjectileRpc(Vector3 position, Vector3 direction, float speed)
    {
        if (gameObject == null) return;

        gameObject.SetActive(true);

        lifeTime = bulletScriptableObject.maxLifeTime;
        transform.position = position;
        transform.right = direction;

        rigidbody.linearVelocity = direction * speed;

        if(IsServer)
        {
            canDamageAttacker = false;
        }
    }

    public void DespawnProjectile()
    {
        if (!IsServer) return;
        if(NetworkObject != null)
            NetworkObject.Despawn();
    }





    public static Projectile CreateNewProjectile(BulletScriptableObject bulletScriptableObject, Character attacker, bool despawnOnDestroy)
    {
        if (!NetworkManager.Singleton.IsServer) return null;

        Projectile projectile = SpawnProjectile(bulletScriptableObject, attacker, despawnOnDestroy);

        return projectile;
    }

    private static Projectile SpawnProjectile(BulletScriptableObject bulletScriptableObject, Character attacker, bool despawnOnDestroy)
    {
        Projectile projectile = Instantiate(bulletScriptableObject.bulletPrefab).GetComponent<Projectile>();

        projectile.bulletScriptableObject = bulletScriptableObject;
        projectile.damage = bulletScriptableObject.damage;
        projectile.maxDamageMultiplier = bulletScriptableObject.maxDamageMultiplier;
        projectile.destroyOnImpact = bulletScriptableObject.destroyOnImpact;
        projectile.attacker = attacker;
        projectile.despawnOnDisable = despawnOnDestroy;

        projectile.NetworkObject.SpawnWithOwnership(NetworkManager.ServerClientId, true);

        return projectile;
    }
}
