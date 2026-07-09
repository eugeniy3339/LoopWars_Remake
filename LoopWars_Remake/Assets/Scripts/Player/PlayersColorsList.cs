using LoopWars.Players;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayersColorsList", menuName = "Players/Create new Player Colors List")]
public class PlayersColorsList : ScriptableObject
{
    public static PlayersColorsList Instance { get { return Resources.Load<PlayersColorsList>("PlayersColorsList"); } }
    public List<Color> colors;
}
