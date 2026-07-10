using UnityEngine;

public class GranadeProjectile : ExplosiveProjectile
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsThereADamagable(collision.gameObject, out IDamagable damagable, out bool isAttacker))
        {
            if (isAttacker)
            {
                if (!bulletScriptableObject.canDamageAttacker || curCanDamageAttackerTimer > 0f)
                    return;
            }

            DestroyOnImpact();
        }
        else if(IsThereAProjectile(collision.gameObject, out Projectile projectile, out bool isAttackers))
        {
            DestroyOnImpact();
        }
    }
}
