using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Weapon : NetworkBehaviour
{
    [SerializeField] private Transform _shootPosition;
    public Transform shootPosition { protected set { _shootPosition = value; } get { return _shootPosition; } }
    private NetworkVariable<int> weaponScriptableObjectIndex = new NetworkVariable<int>();
    private WeaponScriptableObject _wso;
    public virtual WeaponScriptableObject weaponScriptableObject { 
        get { return _wso; }
        set {
            _wso = value;

            bulletScriptableObject = value?.bulletScriptableObject;
            infinityAmmo = value != null ? value.infinityBullets : true;
            ammo = value != null ? value.bulletsCount : 1;

            attackCooldown = value != null ? value.attackCooldown : 1f;

            shotBackForce = value != null ? value.shotBackForce : 1f;
        } 
    }
    protected virtual BulletScriptableObject bulletScriptableObject { get; set; }

    public virtual Character attacker { get; set; }

    protected bool infinityAmmo;
    public int ammo { get; protected set; }
    protected float attackCooldown;
    protected float curAttackCooldown;
    protected float shotBackForce;

    private List<Projectile> spawnedProjectiles;
    private int curProjectileIndex;

    public static event Action<Weapon> onWeaponSpawned;
    public event Action<Weapon> onWeaponDespawned;

    public event Action onAttackCooldown;
    public static event Action<Weapon, WeaponScriptableObject> onShootStatic;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            weaponScriptableObjectIndex.Value = WeaponsListScriptableObject.Instance.weapons.IndexOf(weaponScriptableObject);
        }

        weaponScriptableObjectIndex.OnValueChanged += OnWeaponScriptableObjectIndexChanged;
        OnWeaponScriptableObjectIndexChanged(0, weaponScriptableObjectIndex.Value);

        if (attacker == null)
            attacker = Character.FindCharacterByPlayerId(OwnerClientId);

        if (attacker != null)
            SpawnProjectiles();

        onWeaponSpawned?.Invoke(this);
    }

    public void OnAttackerSpawned(Character attacker)
    {
        if (this.attacker != null) return;

        this.attacker = attacker;
        SpawnProjectiles();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        weaponScriptableObjectIndex.OnValueChanged -= OnWeaponScriptableObjectIndexChanged;
        onWeaponDespawned?.Invoke(this);
    }

    private void OnWeaponScriptableObjectIndexChanged(int oldIndex, int curIndex)
    {
        weaponScriptableObject = WeaponsListScriptableObject.Instance.weapons[curIndex];
    }

    private void SpawnProjectiles()
    {
        int countToSpawn = (int)(bulletScriptableObject.maxLifeTime / attackCooldown);
        SpawnProjectiles(countToSpawn);
    }

    private void SpawnProjectiles(int count)
    {
        spawnedProjectiles = SpawnProjectiles(bulletScriptableObject, count);
        curProjectileIndex = 0;
    }

    private List<Projectile> SpawnProjectiles(BulletScriptableObject bulletScriptableObject, int count)
    {
        List<Projectile> projectiles = new List<Projectile>();

        for(int i = 0; i < count; i++)
        {
            projectiles.Add(Projectile.CreateNewProjectile(this, bulletScriptableObject, attacker, false));
        }

        return projectiles;
    }

    private void Update()
    {
        if(transform.localPosition != Vector3.zero)
            transform.localPosition = Vector2.zero;
        AttackCooldown();
    }

    private void AttackCooldown()
    {
        if (!IsOwner) return;
        if(curAttackCooldown > 0f)
        {
            curAttackCooldown -= Time.deltaTime;
            if(curAttackCooldown <= 0f)
            {
                OnAttackCooldown();
            }
        }
    }

    private void OnAttackCooldown()
    {
        if (!IsOwner) return;
        onAttackCooldown?.Invoke();
    }

    public bool AttackIfCanTo(out bool becauseOfAmmo)
    {
        if (CanAttack(out becauseOfAmmo))
        {
            Attack();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected abstract void Attack();

    [Rpc(SendTo.Everyone)]
    protected virtual void ShootRpc(Vector2 position, Vector2 direction)
    {
        LaunchProjectile(position, direction);
        onShootStatic?.Invoke(this, weaponScriptableObject);
    }

    private void LaunchProjectile(Vector2 position, Vector2 direction)
    {
        if (curProjectileIndex >= spawnedProjectiles.Count)
            curProjectileIndex = 0;

        Projectile projectile = spawnedProjectiles[curProjectileIndex];
        if (projectile == null)
            return;

        projectile.LaunchProjectile(position, direction);
        curProjectileIndex++;
    }

    private bool CanAttack(out bool becauseOfAmmo)
    {
        if (!IsOwner) { becauseOfAmmo = false; return false; }
        becauseOfAmmo = curAttackCooldown <= 0 && !infinityAmmo && ammo <= 0;
        return curAttackCooldown <= 0f && (infinityAmmo || ammo > 0);
    }




    public static Weapon FindWeapon(ulong ownerId)
    {
        foreach(var weapon in FindObjectsOfType<Weapon>())
        {
            if (weapon.OwnerClientId == ownerId)
                return weapon;
        }

        return null;
    }
}
