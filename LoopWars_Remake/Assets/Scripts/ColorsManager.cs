using UnityEngine;
using UnityEngine.U2D;

public static class ColorsManager
{
    public static Color GetTonedColor(Color tones, Color color)
    {
        return new Color(color.r - (1 - tones.r), color.g - (1 - tones.g), color.b - (1 - tones.b));
    }
}
