using UnityEditor;
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
    public SoundScriptableObject bulletDestroySound;
    public SoundsListScriptableObject bulletDestroySounds;
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

#if UNITY_EDITOR

[CustomEditor(typeof(BulletScriptableObject), true)]
public class BulletScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty fireSound = serializedObject.FindProperty("bulletDestroySound");
        SerializedProperty fireSounds = serializedObject.FindProperty("bulletDestroySounds");

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "m_Script")
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(prop);
                EditorGUI.EndDisabledGroup();
                continue;
            }

            if (prop.name == "bulletDestroySound")
            {
                if (fireSounds.boxedValue == null || fireSound.boxedValue != null)
                    EditorGUILayout.PropertyField(prop);
            }
            else if (prop.name == "bulletDestroySounds")
            {
                if (fireSound.boxedValue == null || fireSounds.boxedValue != null)
                    EditorGUILayout.PropertyField(prop);
            }
            else
            {
                EditorGUILayout.PropertyField(prop);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif