using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    private WeaponScriptableObject _wso;
    public virtual WeaponScriptableObject weaponScriptableObject { 
        get { return _wso; }
        set {
            _wso = value;
            bulletScriptableObject = value.bulletScriptableObject;
            infinityAmmo = value.infinityBullets;
            ammo = value.bulletsCount;
            attackCooldown = value.attackCooldown;
        } 
    }
    protected virtual BulletScriptableObject bulletScriptableObject { get; set; }

    public virtual HealthSystem attacker { get; set; }

    protected bool infinityAmmo;
    public int ammo { get; protected set; }
    protected float attackCooldown;
    protected float curAttackCooldown;

    public event Action onAttackCooldown;

    private void Update()
    {
        AttackCooldown();
    }

    private void AttackCooldown()
    {
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
        onAttackCooldown?.Invoke();
    }

    public bool AttackIfCanTo(Vector2 direction, out bool becauseOfAmmo)
    {
        print(CanAttack(out becauseOfAmmo));
        if (CanAttack(out becauseOfAmmo))
        {
            Attack(direction);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected abstract void Attack(Vector2 direction);

    protected virtual Projectile SpawnProjectile(BulletScriptableObject bulletScriptableObject, Vector2 position, Vector2 direction)
    {
        return Projectile.CreateNewProjectile(bulletScriptableObject, attacker, position, direction);
    }

    private bool CanAttack(out bool becauseOfAmmo)
    {
        becauseOfAmmo = curAttackCooldown <= 0 && !infinityAmmo && ammo <= 0;
        return curAttackCooldown <= 0f && (infinityAmmo || ammo > 0);
    }
}
