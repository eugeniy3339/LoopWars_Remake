using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class WeaponsVisualsHandlers
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Weapon.onShootStatic += OnWeaponShoot;
        Projectile.onDestroyedStatic += OnBulletDestroyed;
    }

    private static void OnWeaponShoot(Weapon weapon, WeaponScriptableObject weaponScriptableObject)
    {
        if (weaponScriptableObject?.fireVFXPrefab != null)
            ParticlesHandler.SpawnParticles(weaponScriptableObject.fireVFXPrefab, weapon.shootPosition.position, weapon.shootPosition.right);
    }

    private static void OnBulletDestroyed(Projectile bullet, BulletScriptableObject bulletScriptableObject)
    {
        CameraShakeManager.StartScreenShake(bulletScriptableObject.bulletDestroyScreenShakeTime, bulletScriptableObject.bulletDestroyScreenShakeAmplitude, bulletScriptableObject.bulletDestroyScreenShakeFrequency);
        if (bulletScriptableObject.bulletDestroyParticlesPrefab != null)
            ParticlesHandler.SpawnParticles(bulletScriptableObject.bulletDestroyParticlesPrefab, bullet.transform.position + bullet.transform.right * bullet.collider.size.x / 2f, -bullet.transform.right);
    }
}