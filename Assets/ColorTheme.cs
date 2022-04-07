using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ThemedColor
{
    public ColorTheme.ColorBase type;
    public ColorShade shade;
    [Range(-1,1)]
    [SerializeField]
    float value = 0.8f;

    public float Value => shade == ColorShade.Custom ? value : SetShade();
    public bool complement;
    public Color Color => ColorTheme.instance.GetColor(type, complement, Value);

    public float SetShade()
    {
        switch(shade)
        {
            case ColorShade.XXDark:
                value = ColorTheme.instance.darkTheme ? -0.9f : 0.7f;
                break;
            case ColorShade.XDark:
                value = ColorTheme.instance.darkTheme ? -0.75f : -0.4f;
                break;
            case ColorShade.Dark:
                value = ColorTheme.instance.darkTheme ? -0.5f : -0.7f;
                break;
            case ColorShade.Normal:
                value = 0; 
                break;
            case ColorShade.Light:
                value = ColorTheme.instance.darkTheme ? 0.5f : 0.4f;
                break;
            case ColorShade.XLight:    
                value = ColorTheme.instance.darkTheme ? 0.7f : 0.6f;
                break;
            case ColorShade.XXLight:
                value = ColorTheme.instance.darkTheme ? 0.9f : 0.8f;
                break;
        }

        return value;
    }

    public enum ColorShade
    {
        Custom,
        XXDark,
        XDark,
        Dark,
        Normal, 
        Light,
        XLight,
        XXLight
    }
}





[ExecuteAlways]
public class ColorTheme : MonoBehaviour
{
    public bool darkTheme = true;
    public List<Color> pallette;
    public bool autoGenerateTheme = false;
    public Color neutral;
    public Color accent;
    public Color detail;
    public Color warning;
    public Color success;
    public Color info;

    public Color boardSpaces;
    public Color boardBackground;
    public Color WordMultiply;
    public Color LetterMultiply;

    public static ColorTheme instance;
    public Color sessionPallateColor;

    void Awake()
    {
        if(instance == null) instance = this;
        sessionPallateColor = RandomPalletteColor();
    }

    public void Update()
    {
        if(!Application.isPlaying)
        {
            if(instance == null) instance = this;
            sessionPallateColor = RandomPalletteColor();
        
            if(autoGenerateTheme)
            { 
                GenerateTheme();
            }
        }
    }

    public enum ColorBase
    {
        White,
        Pallette,
        Neutral,
        Accent,
        Detail,
        Warning,
        Success,
        Info,

        BoardSpaces,
        BoardBackground,
        WordMultiply,
        LetterMultiply,
    }

    public Color GetColor(ColorBase baseColor, bool complement, float value = 0.5f)
    {
        Color result = Color.white;
        switch(baseColor)
        {
            case ColorBase.White:
                result = Color.white;
                break;
            case ColorBase.Pallette:
                result = sessionPallateColor;
                break;
            case ColorBase.Neutral:
                result = neutral;
                break;
            case ColorBase.Accent:
                result = accent;
                break;
            case ColorBase.Detail:
                result = detail;
                break;
            case ColorBase.Warning:
                result = warning;
                break;
            case ColorBase.Success:
                result = success;
                break;
            case ColorBase.Info:
                result = info;
                break;
            
            case ColorBase.BoardSpaces:
                result = boardSpaces;
                break;
            case ColorBase.BoardBackground:
                result = boardBackground;
                break;
            case ColorBase.WordMultiply:
                result = WordMultiply;
                break;
            case ColorBase.LetterMultiply:
                result = LetterMultiply;
                break;

        }

        if(complement) result = result.Complement();

        if(value < 0)
        {
            result = darkTheme ? result.Darken(-value) : result.Lighten(1+value + 0.1f);
        }
        else
        {
            result = darkTheme ? result.Lighten(value) : result.Darken(value);
        }

        return result;
    }

    public Color RandomPalletteColor()
    {
        return pallette[Random.Range(0, pallette.Count)];
    }

    public void GenerateTheme()
    {
        //base color is the starting point and is set by the user

        neutral = RandomPalletteColor().OffsetHue(Random.Range(-0.05f, 0.05f)).Saturation(0.4f).Value(0.8f);
        accent = RandomPalletteColor().OffsetHue(Random.Range(-0.1f, 0.1f)).Saturation(0.8f).Value(0.6f);
        detail = RandomPalletteColor().OffsetHue(Random.Range(-0.1f, 0.1f)).Saturation(0.3f).Value(0.7f);

        warning = Color.Lerp(accent, Color.red.Value(0.6f), 0.8f);
        success = Color.Lerp(accent, Color.green.Saturation(0.8f).Value(0.6f), 0.8f);
        info = Color.Lerp(accent, Color.blue.Saturation(0.8f).Value(0.6f), 0.8f);

        boardBackground = Color.Lerp(accent.Darken(0.7f), new Color(0.243f, 0.149f, 0.0666f), 0.3f);
        boardSpaces = Color.Lerp(neutral, new Color(0.788f, 0.564f, 0.411f), 0.6f);
        WordMultiply = neutral.Complement().Saturation(0.5f).Value(0.5f);
        LetterMultiply = WordMultiply.Complement();
    }
}
