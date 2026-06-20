using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Character character;

    private Weapon curWeapon;
    private Vector2 direction;

    [SerializeField] private WeaponScriptableObject weapon;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetCurWeapon(weapon);
    }

    public void SetCurWeapon(WeaponScriptableObject weaponScriptableObject)
    {
        if(curWeapon != null)
            Destroy(curWeapon.gameObject);

        Weapon weapon = Instantiate(weaponScriptableObject.weaponPrefab).GetComponent<Weapon>();
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.weaponScriptableObject = weaponScriptableObject;
        weapon.attacker = character.healthSystem;
        curWeapon = weapon;
    }

    public void StartAttacking()
    {
        curWeapon.onAttackCooldown += OnAttackCooldown;
        Attack();
    }

    public void StopAttacking()
    {
        curWeapon.onAttackCooldown -= OnAttackCooldown;
    }

    private void Attack()
    {
        if (curWeapon == null) { return; }

        if (!curWeapon.AttackIfCanTo(direction, out bool becauseOfBullets) && becauseOfBullets)
        {
            //Play out of bullets sound
            StopAttacking();
        }
    }

    private void OnAttackCooldown()
    {
        Attack();
    }
}
