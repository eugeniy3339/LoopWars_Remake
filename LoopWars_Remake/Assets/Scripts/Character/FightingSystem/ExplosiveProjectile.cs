using System;
using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    protected override BulletScriptableObject bulletScriptableObject { get { return base.bulletScriptableObject; } set { base.bulletScriptableObject = value; explosiveBulletScriptableObject = value as ExplosiveBulletScriptableObject; } }
    protected ExplosiveBulletScriptableObject explosiveBulletScriptableObject;

    public static event Action<ExplosiveProjectile, ExplosiveBulletScriptableObject> onExplodeStatic;

    protected override void OnLifeTime()
    {
        base.OnLifeTime();
        Explode();
    }

    protected override void DestroyOnImpact()
    {
        base.DestroyOnImpact();
        Explode();
    }

    private void Explode()
    {
        foreach(var rigidbody in FindObjectsOfType<Rigidbody2D>())
        {
            if (rigidbody == this.rigidbody) continue;
            Vector2 forceDirection = rigidbody.transform.position - transform.position;
            float distance = forceDirection.magnitude;

            if (distance > explosiveBulletScriptableObject.explosionRange) continue;

            float forceMultiplier = 1 - (distance / explosiveBulletScriptableObject.explosionRange);
            float forceToApply = explosiveBulletScriptableObject.explosionStrength * forceMultiplier;
            rigidbody.AddForce(forceDirection * forceToApply, ForceMode2D.Impulse);

            if(IsThereADamagable(rigidbody.gameObject, out IDamagable damagable, out bool isAttacker))
            {
                Damage(damagable, explosiveBulletScriptableObject.explosionDamage * forceMultiplier);
            }
            else if(IsThereAProjectile(rigidbody.gameObject, out Projectile projectile, out bool isAttackers))
            {
                rigidbody.transform.right = rigidbody.linearVelocity;
            }
        }

        onExplodeStatic?.Invoke(this, explosiveBulletScriptableObject);
    }
}
