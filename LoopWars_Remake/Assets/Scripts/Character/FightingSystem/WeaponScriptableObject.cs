using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponScriptableObject", menuName = "Weapons/Create new Weapon Scriptable Object")]
public class WeaponScriptableObject : ScriptableObject
{
    public GameObject weaponPrefab;
    public float attackCooldown = 1f;
    public bool infinityBullets = false;
    public int bulletsCount = 20;
    public float shotBackForce = 1f;
    public BulletScriptableObject bulletScriptableObject;
    public BulletScriptableObject gunThrowableScriptableObject;
    public GameObject fireVFXPrefab;
}

#if UNITY_EDITOR

[CustomEditor(typeof(WeaponScriptableObject), true)]
public class WeaponScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty infinityBullets = serializedObject.FindProperty("infinityBullets");

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

            if (prop.name == "bulletsCount")
            {
                if (!infinityBullets.boolValue)
                {
                    EditorGUILayout.PropertyField(prop);
                }
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