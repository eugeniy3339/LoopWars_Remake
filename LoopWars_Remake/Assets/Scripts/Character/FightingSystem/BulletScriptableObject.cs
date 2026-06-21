using UnityEngine;

[CreateAssetMenu(fileName = "newBulletScriptableObject", menuName = "Weapons/Create new Bullet Scriptable Object")]
public class BulletScriptableObject : ScriptableObject
{
    public GameObject bulletPrefab;
    public float damage = 10f;
    public float maxDamageMultiplier = 1f;
    public float speed = 10f;
    public float maxLifeTime = 3f;
    //public float wallImpactScreenShakeMultiplier;
    public bool destroyOnImpact = true;


#if UNITY_EDITOR

    private void OnValidate()
    {
        if(maxDamageMultiplier < 1f) maxDamageMultiplier = 1f;
    }

#endif
}
