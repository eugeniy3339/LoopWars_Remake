using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapsListScriptableObject", menuName = "Maps/Create new Maps List")]
public class MapsListScriptableObject : ScriptableObject
{
    public static MapsListScriptableObject Instance { get { return Resources.Load<MapsListScriptableObject>("DefaultMapsListScriptableObject"); } }
    public List<GameObject> maps = new List<GameObject>();

    public GameObject GetRandomMap()
    {
        return maps[Random.Range(0, maps.Count)];
    }
}
