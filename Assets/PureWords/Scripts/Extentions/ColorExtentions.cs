using UnityEngine;

public static class ColorExtentions
{
    public static float Wrap255(float input, float offset)
    {
        return (input + offset) * 255 % 255 / 255;
    }

    public static Color OffsetHue(this Color color, float offset)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return Color.HSVToRGB(Wrap255(h, offset), s, v);
    }

    public static Color Complement(this Color color)
    {
        return color.OffsetHue(0.5f);
    }

    public static Color Multiply(this Color color, float amount)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }

    public static Color Darken(this Color color, float amount)
    {
        return Color.Lerp(color, Color.black, amount);
    }

    public static Color Lighten(this Color color, float amount)
    {
        return Color.Lerp(color, Color.white, amount);
    }

    public static Color Saturation(this Color color, float value)
    {
        Color.RGBToHSV(color, out float h, out float _, out float v);
        return Color.HSVToRGB(h, Mathf.Clamp01(value), v);
    }

    public static Color Hue(this Color color, float value)
    {
        Color.RGBToHSV(color, out float _, out float s, out float v);
        return Color.HSVToRGB(Mathf.Clamp01(value), s, v);
    }

    public static Color Value(this Color color, float value)
    {
        Color.RGBToHSV(color, out float h, out float s, out float _);
        return Color.HSVToRGB(h, s, Mathf.Clamp01(value));
    }
    public static Color AddSaturation(this Color color, float value)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return Color.HSVToRGB(h, Mathf.Clamp01(s + value), v);
    }
}