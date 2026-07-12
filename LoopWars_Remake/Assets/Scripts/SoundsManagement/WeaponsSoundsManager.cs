using UnityEngine;
using UnityEngine.TextCore.Text;

public static class WeaponsSoundsManager
{
    #region Sounds
    private static SoundScriptableObject throwSound;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        throwSound = Resources.Load<SoundScriptableObject>("Sounds/Throw");

        Weapon.onShootStatic += OnWeaponShoot;
        Projectile.onDestroyedStatic += OnBulletDestroyed;
        WeaponManager.onWeaponGotThrown += OnWeaponGotThrown;
    }

    private static void OnWeaponShoot(Weapon weapon, WeaponScriptableObject weaponScriptableObject)
    {
        try
        {
            if (weaponScriptableObject.fireSound == null) return;
            SoundsManager.StartSound(weaponScriptableObject.fireSound, null);
        }
        catch
        {

        }
    }

    private static void OnBulletDestroyed(Projectile projectile, BulletScriptableObject bulletScriptableObject)
    {
        try
        {
            if (bulletScriptableObject.bulletDestroySound == null) return;
            SoundsManager.StartSound(bulletScriptableObject.bulletDestroySound, null);
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
            SoundsManager.StartSound(throwSound, character.transform);
        }
        catch
        {

        }
    }
}
