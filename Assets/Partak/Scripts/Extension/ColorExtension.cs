using UnityEngine;
using System.Collections;

public static class ColorExtension
{
    public static Color SetRGB(this Color color, Color input)
    {
        color.r = input.r;
        color.g = input.g;
        color.b = input.b;
        return color;
    }

    public static bool RGBEquals(this Color color, Color input)
    {
        return Mathf.Approximately(color.r, input.r) && 
               Mathf.Approximately(color.g, input.g) &&
               Mathf.Approximately(color.b, input.b);
    }
}