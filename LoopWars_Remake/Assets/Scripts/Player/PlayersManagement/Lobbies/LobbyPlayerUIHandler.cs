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
        colorIndicatorImage.color = color;
        SetReadyState(ready);
    }

    public void SetReadyState(bool ready)
    {
        readyText.text = ready ? "Ready" : "Unready";
    }
}
