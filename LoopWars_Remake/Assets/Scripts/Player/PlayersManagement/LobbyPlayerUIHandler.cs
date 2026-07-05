using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Image colorIndicatorImage;

    public void SetUp(string name, bool ready, Color color)
    {
        nameText.text = name;
        readyText.text = ready ? "Ready" : "Unready";
        colorIndicatorImage.color = color;
    }
}
