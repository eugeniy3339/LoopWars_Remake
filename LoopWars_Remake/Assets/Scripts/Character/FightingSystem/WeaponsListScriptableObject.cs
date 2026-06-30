using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponsList", menuName = "Weapons/Create new Weapons List")]
public class WeaponsListScriptableObject : ScriptableObject
{
    public List<WeaponScriptableObject> weapons = new List<WeaponScriptableObject>();
}
