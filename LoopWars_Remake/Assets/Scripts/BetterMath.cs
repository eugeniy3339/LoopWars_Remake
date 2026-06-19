using UnityEngine;

public struct BetterMath
{
    public static float NormalizedFloat(float value)
    {
        if (value == 0f)
            return 0f;
        else
            return value / Mathf.Abs(value);
    }
}
