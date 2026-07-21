using UnityEngine;
using UnityEngine.TextCore.Text;

public static class WeaponsSoundsManager
{
    #region Sounds
    private static SoundsListScriptableObject throwSound;
    private static SoundScriptableObject explosionSound;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        throwSound = Resources.Load<SoundsListScriptableObject>("Sounds/Throw");
        explosionSound = Resources.Load<SoundScriptableObject>("Sounds/Explosion");

        Weapon.onShootStatic += OnWeaponShoot;
        Projectile.onDestroyedStatic += OnBulletDestroyed;
        WeaponManager.onWeaponGotThrown += OnWeaponGotThrown;
        ExplosiveProjectile.onExplodeStatic += OnExplosion;
    }

    private static void OnWeaponShoot(Weapon weapon, WeaponScriptableObject weaponScriptableObject)
    {
        try
        {
            if (weaponScriptableObject.fireSound == null)
            {
                if (weaponScriptableObject.fireSounds == null) return;
                SoundsManager.StartSound(weaponScriptableObject.fireSounds, null);
            }
            else
            {
                SoundsManager.StartSound(weaponScriptableObject.fireSound, null);
            }
        }
        catch
        {

        }
    }

    private static void OnBulletDestroyed(Projectile projectile, BulletScriptableObject bulletScriptableObject)
    {
        try
        {
            if (bulletScriptableObject.bulletDestroySound == null)
            {
                if (bulletScriptableObject.bulletDestroySounds == null) return;
                SoundsManager.StartSound(bulletScriptableObject.bulletDestroySounds, null);
            }
            else
            {
                SoundsManager.StartSound(bulletScriptableObject.bulletDestroySound, null);
            }
        }
        catch
        {

        }
    }

    private static void OnWeaponGotThrown(Character character, WeaponScriptableObject weaponScriptableObject)
    {
        try
        {
            if (throwSound == null) return;
            SoundsManager.StartSound(throwSound, null);
        }
        catch
        {

        }
    }

    public static void OnExplosion(ExplosiveProjectile projectile, ExplosiveBulletScriptableObject projectileScriptableObject)
    {
        try
        {
            if (explosionSound == null) return;
            SoundsManager.StartSound(explosionSound, null);
        }
        catch
        {

        }
    }
}
