using LoopWars.Players;
using TMPro;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public int score {
        set
        {
            scoreText.text = value.ToString();
        }
    }
    public Color color
    {
        set
        {
            scoreText.color = ColorsManager.GetTonedColor(scoreText.color, value);
        }
    }
}
