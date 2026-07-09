using UnityEngine;
using System.Collections.Generic;
using LoopWars.Players;

public class ObjectColorsHandler : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spritesToColor;

    public void ColorSprites(Color color)
    {
        foreach(var spriteToColor in spritesToColor)
        {
            ColorTheSprite(color, spriteToColor);
        }
    }

    private void ColorTheSprite(Color color, SpriteRenderer sprite)
    {
        sprite.color = ColorsManager.GetTonedColor(sprite.color, color);
    }
}
