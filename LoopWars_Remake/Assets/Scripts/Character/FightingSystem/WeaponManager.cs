using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    private Character _char;
    private Character character { get { if (_char == null) { _char = GetComponent<Character>(); } return _char; } set { _char = value; } }

    [SerializeField] private WeaponsListScriptableObject weaponsList;

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
                curWeapon.transform.right = value;
        }
    }
    private bool subscribedToCooldownEvent = false;

    [SerializeField] private WeaponScriptableObject weapon;

    private void Awake()
    {
        character = GetComponent<Character>();
        direction = Vector2.right;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) { enabled = false; return; }

        SetCurWeapon(weapon);
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
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.right = direction;

        return weapon;
    }

    public void DestroyCurWeapon()
    {
        if (!IsServer) return;
        if (curWeapon == null) return;

        StopAttacking();
        curWeapon.NetworkObject.Despawn();
        curWeaponScriptableObject = null;
        curWeapon = null;
    }

    public void ThrowWeapon()
    {
        if (!IsOwner) return;

        ThrowWeaponRpc();
    }

    [Rpc(SendTo.Server)]
    private void ThrowWeaponRpc()
    {
        if (curWeapon == null) return;

        Projectile projectile = Projectile.CreateNewProjectile(curWeaponScriptableObject.gunThrowableScriptableObject, character, true);
        projectile.LaunchProjectileRpc(curWeapon.transform.position, curWeapon.transform.right, curWeaponScriptableObject.gunThrowableScriptableObject.speed);

        DestroyCurWeapon();
    }

    public void StartAttacking() //Owner Player Input
    {
        if (!IsOwner) return;

        StartAttackingRpc();
    }

    [Rpc(SendTo.Server)]
    private void StartAttackingRpc() //Server Rpc
    {
        if (curWeapon == null) return;

        SubscribeOnCooldownEvent();
        Attack();
    }

    public void StopAttacking() //Owner Player Input
    {
        if (!IsOwner) return;
            
        StopAttackingRpc();
    }

    [Rpc(SendTo.Server)]
    private void StopAttackingRpc() //Server Rpc
    {
        if (curWeapon == null) return;

        UnsubscribeFromCooldownEvent();
    }

    private void Attack() // Server Function
    {
        if (!IsServer) return;
        if (curWeapon == null) { return; }

        if (!curWeapon.AttackIfCanTo(out bool becauseOfBullets) && becauseOfBullets)
        {
            //Play out of bullets sound
            StopAttacking();
        }
    }

    private void SubscribeOnCooldownEvent()
    {
        if (!IsServer) return;
        if (!subscribedToCooldownEvent)
        {
            curWeapon.onAttackCooldown += OnAttackCooldown;
            subscribedToCooldownEvent = true;
        }
    }

    private void UnsubscribeFromCooldownEvent()
    {
        if (!IsServer) return;
        curWeapon.onAttackCooldown -= OnAttackCooldown;
        subscribedToCooldownEvent = false;
    }

    private void OnAttackCooldown()
    {
        if (!IsServer) return;
        Attack();
    }
}
