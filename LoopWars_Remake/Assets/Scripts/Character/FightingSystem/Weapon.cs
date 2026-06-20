using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    private int ammo;
    private float attackCooldown;
    private float curAttackCooldown;

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

    protected virtual void SpawnProjectile(Vector2 direction)
    {

    }

    private bool CanAttack(out bool becauseOfAmmo)
    {
        becauseOfAmmo = curAttackCooldown <= 0 && ammo <= 0;
        return curAttackCooldown <= 0 && ammo > 0;
    }
}
