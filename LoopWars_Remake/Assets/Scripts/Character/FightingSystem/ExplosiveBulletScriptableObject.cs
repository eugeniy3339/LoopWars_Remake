using UnityEngine;

[CreateAssetMenu(fileName = "newBulletScriptableObject", menuName = "Weapons/Create new Explosive Bullet Scriptable Object")]
public class ExplosiveBulletScriptableObject : BulletScriptableObject
{
    public GameObject explosionParticles;
    public float explosionRange = 3f;
    public float explosionStrength = 3f;
    public float explosionDamage = 50f;
}
