using UnityEngine;

[CreateAssetMenu(fileName = "newBulletScriptableObject", menuName = "Weapons/Create new Bullet Scriptable Object")]
public class BulletScriptableObject : ScriptableObject
{
    public GameObject bulletPrefab;
    public float damage = 10f;
    public float maxDamageMultiplier = 1f;
    public float speed = 10f;
    public float maxLifeTime = 3f;
    public bool canDamageAttacker = false;
    public bool canBeDestroyedByAttackersProjectiles = false;
    public bool destroyOnImpact = true;
    public GameObject bulletDestroyParticlesPrefab;
    public float bulletDestroyScreenShakeTime = 0.2f;
    public float bulletDestroyScreenShakeAmplitude = 1f;
    public float bulletDestroyScreenShakeFrequency = 2f;


#if UNITY_EDITOR

    private void OnValidate()
    {
        if(maxDamageMultiplier < 1f) maxDamageMultiplier = 1f;
    }

#endif
}
