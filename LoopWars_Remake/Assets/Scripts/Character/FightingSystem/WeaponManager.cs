using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Character character;

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

    private void Start()
    {
        SetCurWeapon(weapon);
    }

    public bool SetCurWeapon(WeaponScriptableObject weaponScriptableObject)
    {
        if (curWeapon != null) return false;
        if (weaponScriptableObject == null) return false;
        //DestroyCurWeapon();

        curWeaponScriptableObject = weaponScriptableObject;
        Weapon weapon = SpawnWeapon(weaponScriptableObject);
        curWeapon = weapon;
        subscribedToCooldownEvent = false;
        return true;
    }

    public void DestroyCurWeapon()
    {
        if (curWeapon == null) return;

        StopAttacking();
        Destroy(curWeapon.gameObject);
        curWeaponScriptableObject = null;
    }

    public void ThrowWeapon()
    {
        if (curWeapon == null) return;

        Projectile.CreateNewProjectile(curWeaponScriptableObject.gunThrowableScriptableObject, character, curWeapon.transform.position, curWeapon.transform.right);
        DestroyCurWeapon();
    }

    public void StartAttacking()
    {
        if (curWeapon == null) return;

        SubscribeOnCooldownEvent();
        Attack();
    }

    public void StopAttacking()
    {
        if (curWeapon == null) return;

        UnsubscribeFromCooldownEvent();
    }

    private void Attack()
    {
        if (curWeapon == null) { return; }

        if (!curWeapon.AttackIfCanTo(out bool becauseOfBullets) && becauseOfBullets)
        {
            //Play out of bullets sound
            StopAttacking();
        }
    }

    private Weapon SpawnWeapon(WeaponScriptableObject weaponScriptableObject)
    {
        Weapon weapon = Instantiate(weaponScriptableObject.weaponPrefab).GetComponent<Weapon>();
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.right = direction;
        weapon.weaponScriptableObject = weaponScriptableObject;
        weapon.attacker = character;

        return weapon;
    }

    private void SubscribeOnCooldownEvent()
    {
        if (!subscribedToCooldownEvent)
        {
            curWeapon.onAttackCooldown += OnAttackCooldown;
            subscribedToCooldownEvent = true;
        }
    }

    private void UnsubscribeFromCooldownEvent()
    {
        curWeapon.onAttackCooldown -= OnAttackCooldown;
        subscribedToCooldownEvent = false;
    }

    private void OnAttackCooldown()
    {
        print("onAttackCooldown");
        Attack();
    }
}
