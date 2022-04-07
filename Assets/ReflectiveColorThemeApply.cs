using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

[Serializable]
public class ReflectiveColorElement
{
    public Component component;
    public string colorFields;
    public ThemedColor color;

    public void SetColor()
    {
        if(component == null) return;
        var split = colorFields.Replace(" ", "").Split(',');
        if (split.Length == 0)
        {
            split = new string[] { colorFields };
        }
        foreach(var str in split)
        {
            var property = component.GetType().GetProperty(str);
            if(property != null)
            {
                property.SetValue(component, color.Color);
            }
            else
            {
                var field = component.GetType().GetField(str);
                if(field != null)
                {
                    field.SetValue(component, color.Color);
                }
            }
        }
    }

}

[ExecuteAlways]
public class ReflectiveColorThemeApply : MonoBehaviour
{
    public List<ReflectiveColorElement> colors = new List<ReflectiveColorElement>();


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
            for(int i = 0; i < colors.Count; i++)
            {
                colors[i].SetColor();
            }
        }
    }
}
