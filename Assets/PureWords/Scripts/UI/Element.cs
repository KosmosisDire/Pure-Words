using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.UI.StackLayout{

[Serializable]
public class Element : MonoBehaviour
{
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public StackLayout nestedLayout;
    [HideInInspector] public Section section;
    public bool HasNestedLayout {get{if(nestedLayout == null) nestedLayout = rectTransform.GetComponent<StackLayout>(); return nestedLayout != null;}}
    public Vector2 alignment = Vector2.one/2;
    public bool autoWidth = true;
    public bool autoHeight = true;
    //public bool square = false;
    [Range(0,1)]
    public float squarness = 0;
    public bool autoPosX = true;
    public bool autoPosY = true;
    [SerializeField]
    float extraSpacing = 0;
    public StackLayout.SpacingMode spacingMode = StackLayout.SpacingMode.AbsolutePixels;

    public float ExtraSpacing
    {
        get
        {
            if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if(rectTransform.parent.GetComponent<RectTransform>() == null) Debug.LogError("StackLayout: Element has no parent RectTransform");

            return spacingMode switch
            {
                StackLayout.SpacingMode.AbsolutePixels => extraSpacing,
                StackLayout.SpacingMode.ProportionalToSelf => extraSpacing * (section.layout.Horizontal ? rectTransform.rect.size.x : rectTransform.rect.size.y),
                StackLayout.SpacingMode.ProportionalToParent => extraSpacing * (section.layout.Horizontal ? rectTransform.parent.GetComponent<RectTransform>().rect.size.x : rectTransform.parent.GetComponent<RectTransform>().rect.size.y),
                StackLayout.SpacingMode.ProportionalToScreen => extraSpacing * (section.layout.Horizontal ? Screen.currentResolution.width : Screen.currentResolution.height),
                _ => extraSpacing,
            };
        }
    }

    [HideInInspector]
    public bool init = false;

    public Element Init()
    {
        rectTransform = GetComponent<RectTransform>();
        nestedLayout = rectTransform.GetComponent<StackLayout>();

        if(init) return this;

        alignment = Vector2.one/2;
        autoWidth = true;
        autoHeight = true;
        autoPosX = true;
        autoPosY = true;
        extraSpacing = 0;
        init = true;

        return this;
    }

}

}
