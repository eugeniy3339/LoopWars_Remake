using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponsList", menuName = "Weapons/Create new Weapons List")]
public class WeaponsListScriptableObject : ScriptableObject
{
    public static WeaponsListScriptableObject Instance { get { return Resources.Load<WeaponsListScriptableObject>("DefaultWeaponsList"); } }
    public List<WeaponScriptableObject> weapons = new List<WeaponScriptableObject>();
}
