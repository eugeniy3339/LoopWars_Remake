using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    private Character _char;
    private Character character { get { if (_char == null) { _char = GetComponent<Character>(); } return _char; } set { _char = value; } }

    private WeaponScriptableObject curWeaponScriptableObject;
    private Weapon curWeapon;
    private Vector2 _d;
    public Vector2 direction
    {
        get
        {
            return _d;
        }
        set
        {
            _d = value;

            if(curWeapon != null)
            {
                curWeapon.transform.right = value;
            }
        }
    }

    private bool subscribedToCooldownEvent = false;
    private bool subscribedToWeaponSpawnEvent = false;

    [SerializeField] private WeaponScriptableObject startWeapon;

    private void Awake()
    {
        character = GetComponent<Character>();
        direction = Vector2.right;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SetCurWeapon(startWeapon);
        }
        else
        {
            if(curWeapon == null)
                curWeapon = Weapon.FindWeapon(OwnerClientId);
            curWeapon.OnAttackerSpawned(character);
            if(curWeaponScriptableObject == null)
                curWeaponScriptableObject = curWeapon.weaponScriptableObject;
        }

        if (!IsOwner)
        {
            enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer) return;
        if (curWeapon != null && curWeapon.NetworkObject != null && curWeapon.NetworkObject.IsSpawned)
            curWeapon.NetworkObject.Despawn();
    }

    public bool SetCurWeapon(WeaponScriptableObject weaponScriptableObject)
    {
        if (!IsServer) return false;
        if (weaponScriptableObject == null) return false;
        if (curWeapon != null) return false;

        curWeaponScriptableObject = weaponScriptableObject;
        Weapon weapon = SpawnWeapon(weaponScriptableObject);
        curWeapon = weapon;
        subscribedToCooldownEvent = false;
        return true;
    }

    private Weapon SpawnWeapon(WeaponScriptableObject weaponScriptableObject)
    {
        if (!IsServer) return null;
        Weapon weapon = Instantiate(weaponScriptableObject.weaponPrefab).GetComponent<Weapon>();
        weapon.weaponScriptableObject = weaponScriptableObject;
        weapon.attacker = character;
        weapon.NetworkObject.SpawnWithOwnership(OwnerClientId, true);
        weapon.transform.SetParent(transform);

        return weapon;
    }

    [Rpc(SendTo.Everyone)]
    public void DestroyCurWeaponRpc()
    {
        DestroyCurWeapon();
    }

    public void DestroyCurWeapon()
    {
        if(IsOwner)
            StopAttacking();

        if (IsServer && curWeapon != null && curWeapon.NetworkObject.IsSpawned)
            curWeapon.NetworkObject.Despawn();

        curWeaponScriptableObject = null;
        curWeapon = null;
    }

    public void ThrowWeapon()
    {
        if (!IsOwner) return;
        if (curWeaponScriptableObject == null) return;

        ThrowWeaponRpc(WeaponsListScriptableObject.Instance.weapons.IndexOf(curWeaponScriptableObject), curWeapon.transform.position, curWeapon.transform.right);
    }

    [Rpc(SendTo.Everyone)]
    private void ThrowWeaponRpc(int weponScriptableObject, Vector2 position, Vector2 direction)
    {
        if (WeaponsListScriptableObject.Instance.weapons[weponScriptableObject] == null) return;
        Projectile projectile = Projectile.CreateNewProjectile(WeaponsListScriptableObject.Instance.weapons[weponScriptableObject].gunThrowableScriptableObject, character, true);
        projectile.LaunchProjectile(position, direction);

        DestroyCurWeapon();
    }

    public void StartAttacking() //Owner Player Input
    {
        if (!IsOwner) return;
        if (curWeapon == null) return;

        SubscribeOnCooldownEvent();
        Attack();
    }

    public void StopAttacking() //Owner Player Input
    {
        if (!IsOwner) return;
        if (curWeapon == null) return;

        UnsubscribeFromCooldownEvent();
    }

    private void Attack() // Server Function
    {
        if (!IsOwner) return;
        if (curWeapon == null) return;

        if (!curWeapon.AttackIfCanTo(out bool becauseOfBullets) && becauseOfBullets)
        {
            //Play out of bullets sound
            StopAttacking();
        }
    }

    private void SubscribeOnCooldownEvent()
    {
        if (!IsOwner) return;
        if (!subscribedToCooldownEvent)
        {
            curWeapon.onAttackCooldown += OnAttackCooldown;
            subscribedToCooldownEvent = true;
        }
    }

    private void UnsubscribeFromCooldownEvent()
    {
        if (!IsOwner) return;
        curWeapon.onAttackCooldown -= OnAttackCooldown;
        subscribedToCooldownEvent = false;
    }

    private void OnAttackCooldown()
    {
        if (!IsOwner) return;
        Attack();
    }

    private void OnWeaponSpawned(Weapon weapon)
    {
        if (curWeapon == null && weapon.OwnerClientId == OwnerClientId)
        {
            curWeapon = weapon;
            curWeaponScriptableObject = weapon.weaponScriptableObject;
        }
    }

    private void OnEnable()
    {
        Weapon.onWeaponSpawned += OnWeaponSpawned;
    }

    private void OnDisable()
    {
        Weapon.onWeaponSpawned -= OnWeaponSpawned;
    }
}
