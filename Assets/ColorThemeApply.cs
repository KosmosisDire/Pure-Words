using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ColorElement
{
    public Graphic graphic;
    public ThemedColor color;

    public void SetColor()
    {
        if(graphic == null || color == null) return;
        graphic.color = color.Color * graphic.color.a;
    }
}

[ExecuteAlways]
public class ColorThemeApply : MonoBehaviour
{
    public List<ColorElement> colors = new List<ColorElement>();


    void Start()
    {
        for(int i = 0; i < colors.Count; i++)
        {
            colors[i].SetColor();
        }
    }

    void Update()
    {
        if(!Application.isPlaying)
        {
            var graphics = new List<Graphic>(GetComponentsInChildren<Graphic>(true));
            for(int i = 0; i < graphics.Count; i++)
            {
                if(i >= colors.Count) colors.Add(new ColorElement());
                if(colors.Any(c => c.graphic == graphics[i])) continue;
                colors[i].graphic = graphics[i];
            }

            for(int i = 0; i < colors.Count; i++)
            {
                colors[i].SetColor();
            }
        }
    }
}
