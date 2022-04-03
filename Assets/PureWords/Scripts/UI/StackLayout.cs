using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace UnityEngine.UI.StackLayout{

public enum Overflow
{
    Sit,
    Extend,
    Scale
}

[Serializable]
public struct Margins
{
    public float top;
    public float left;
    public float bottom;
    public float right;
    

    public Vector2 TopLeft => new Vector2(left, top);
    public Vector2 TopRight => new Vector2(right, top);
    public Vector2 BottomLeft => new Vector2(left, bottom);
    public Vector2 BottomRight => new Vector2(right, bottom);
    
    public Margins (float left, float right, float top, float bottom)
    {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
    }

    public static Margins operator *(Margins margins, float scalar)
    {
        return new Margins(margins.left * scalar, margins.right * scalar, margins.top * scalar, margins.bottom * scalar);
    }

    public static Margins operator *(Margins margins, Vector2 scalar)
    {
        return new Margins(margins.left * scalar.x, margins.right * scalar.x, margins.top * scalar.y, margins.bottom * scalar.y);
    }
}



[ExecuteAlways]
public class StackLayout : MonoBehaviour
{

    #region Enums and Variables
    public enum SpacingMode
    {
        AbsolutePixels,
        ProportionalToSelf,
        ProportionalToParent,
        ProportionalToScreen
    }
    public enum MarginsMode
    {
        Scalar,
        VerticalHorizontal,
        TopLeftBottomRight
    }
    public enum SpacingMeasurmentDirection
    {
        Both,
        Horizonatal,
        Vertical, 
        Larger,
        Smaller
    }
    [Header("Items")]
    public bool UseChildren = true;
    public List<Element> elements = new List<Element>();

    [Space(30)]
    [Header("Direction and Overflow")]
    public RectTransform.Axis stackDirection = RectTransform.Axis.Vertical;
    public Overflow overflowBehavior = Overflow.Extend;
    public Overflow underflowBehavior = Overflow.Extend;

    [Space(30)]
    [Header("Spacing")]
    public SpacingMode globalSpacingMode;
    [SerializeField]
    float globalSpacing = 0;
    public bool autoSetGlobalSpacing = false;
    [Range(0,10)]
    public float spacingCorrectionSpeed = 2f;
    public float GlobalSpacing
    {
        get
        {
            if(autoSetGlobalSpacing) 
            {

                if(underflowBehavior == Overflow.Extend && elements.Count > 1)
                {
                    if(Horizontal && PixelOverflow.x < -1)
                    {
                        globalSpacing += -PixelOverflow.x/elements.Count *  Mathf.Min(Time.deltaTime * spacingCorrectionSpeed,0.5f);
                    }
                    else if(Vertical && PixelOverflow.y < -1)
                    {
                        globalSpacing += -PixelOverflow.y/elements.Count *  Mathf.Min(Time.deltaTime * spacingCorrectionSpeed,0.5f);
                    }
                }

                if(overflowBehavior == Overflow.Extend || globalSpacing > (Margins.top + Margins.bottom + Margins.left + Margins.right)/4)
                {
                    if(Horizontal && PixelOverflow.x > 1)
                    {
                        globalSpacing += -PixelOverflow.x/elements.Count * Mathf.Min(Time.deltaTime * spacingCorrectionSpeed,0.5f);
                    }
                    else if(Vertical && PixelOverflow.y > 1)
                    {
                        globalSpacing += -PixelOverflow.y/elements.Count *  Mathf.Min(Time.deltaTime * spacingCorrectionSpeed,0.5f);
                    }
                }

                if(underflowBehavior != Overflow.Extend && overflowBehavior != Overflow.Extend && globalSpacing < (Margins.top + Margins.bottom + Margins.left + Margins.right)/4)
                {
                    globalSpacing = (Margins.top + Margins.bottom + Margins.left + Margins.right)/4;
                }

                globalSpacingMode = SpacingMode.AbsolutePixels;
            }

            return globalSpacingMode switch
            {
                SpacingMode.AbsolutePixels => globalSpacing,
                SpacingMode.ProportionalToSelf => globalSpacing * (Horizontal ? GetComponent<RectTransform>().rect.size.x : GetComponent<RectTransform>().rect.size.y),
                SpacingMode.ProportionalToParent => globalSpacing * (Horizontal ? transform.parent.GetComponent<RectTransform>().rect.size.x : transform.parent.GetComponent<RectTransform>().rect.size.y),
                SpacingMode.ProportionalToScreen => globalSpacing * (Horizontal ? Screen.currentResolution.width : Screen.currentResolution.height),
                _ => globalSpacing,
            };
        }
    }

    
    [Space(30)]
    [Header("Margins")]
    
    public SpacingMode marginSpacingMode;
    public MarginsMode marginsMode;
    public SpacingMeasurmentDirection marginMeasurmentDirection;

    [SerializeField]
    Margins margins;
    
    public Margins Margins 
    {
        get
        {
            switch (marginsMode)
            {
                case MarginsMode.Scalar:
                    margins.left = margins.right = margins.bottom = margins.top;
                    break;
                case MarginsMode.VerticalHorizontal:
                    margins.right = margins.left;
                    margins.bottom = margins.top;
                    break;
            }

            Vector2 space = marginSpacingMode switch
            {
                SpacingMode.AbsolutePixels => Vector2.one,
                SpacingMode.ProportionalToSelf => GetComponent<RectTransform>().rect.size,
                SpacingMode.ProportionalToParent => transform.parent.GetComponent<RectTransform>().rect.size,
                SpacingMode.ProportionalToScreen => new Vector2(Screen.currentResolution.width, Screen.currentResolution.height),
                _ => Vector2.one,
            };

            Vector2 measureSpace = marginMeasurmentDirection switch
            {
                SpacingMeasurmentDirection.Both => space,
                SpacingMeasurmentDirection.Horizonatal => new Vector2(space.x, space.x),
                SpacingMeasurmentDirection.Vertical => new Vector2(space.y, space.y),
                SpacingMeasurmentDirection.Larger => new Vector2(Mathf.Max(space.x, space.y), Mathf.Max(space.x, space.y)),
                SpacingMeasurmentDirection.Smaller => new Vector2(Mathf.Min(space.x, space.y), Mathf.Min(space.x, space.y)),
                _ => Vector2.one,
            };

            return margins * measureSpace;
        }
    }

    public bool updateEveryFrame = false;
    public int updateInterval = 10;
    public bool independentOfParent = false;
    
    [HideInInspector]
    public List<Section> sections = new List<Section>();
    [HideInInspector]
    public RectTransform rectTransform;
    

    public Vector2 Size => rectTransform.rect.size - Margins.TopLeft - Margins.BottomRight;
    public bool Horizontal => stackDirection == RectTransform.Axis.Horizontal;
    public bool Vertical => stackDirection == RectTransform.Axis.Vertical;
    public Vector2 PixelOverflow => sections.Last().GetEndingPixelOffset() - rectTransform.rect.size + Margins.BottomRight;
    public Vector2 NormalElementSize => new Vector2(Size.x / (Horizontal ? elements.Count : 1), Size.y / (Vertical ? elements.Count : 1));

    #endregion

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void BuildSections()
    {
        sections.Clear();
        Section currentSection = new Section(this, null);
        sections.Add(currentSection);

        for (int i = 0; i < elements.Count; i++)
        {
            Element element = elements[i];
            if((!element.autoPosX && Horizontal) || (!element.autoPosY && Vertical))
            {
                if(!element.autoPosX) element.autoWidth = false;
                if(!element.autoPosY) element.autoHeight = false;

                currentSection.intermediateLast = element;
                currentSection.sectionAfter = new Section(this, currentSection);
                currentSection = currentSection.sectionAfter;
                sections.Add(currentSection);
                currentSection.intermediateFirst = element;
                continue;
            }
            
            currentSection.Add(element.Init());
            element.section = currentSection;
        }
    }

    public void BuildStack()
    {
        BuildSections();

        foreach (Section section in sections)
        {
            if(!section.BuildStack()) return;
        }
    }

    public Vector2 lastResolution = new Vector2(Screen.width, Screen.height);
    public int iterationsAfterChange = 0;
    public float timer = -1;
    public void LateUpdate()
    {  
        if(lastResolution != new Vector2(Screen.width, Screen.height) || iterationsAfterChange > 0)
        { 
            if(iterationsAfterChange == 0)
            {
                iterationsAfterChange = 10;
                lastResolution = new Vector2(Screen.width, Screen.height);
            }

            timer = -1;
            iterationsAfterChange--;
        }

        if(timer > 0 && !updateEveryFrame)
        {
            timer -= Time.deltaTime;
            return;
        }
        
        

        if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if(UseChildren)
        {
            elements.Clear();
            foreach(Transform child in transform)
            {
                if(!child.GetComponent<Element>()) child.gameObject.AddComponent<Element>();
                elements.Add(child.GetComponent<Element>());
            }
        }

        timer = updateInterval;
        
        if(!independentOfParent && GetComponent<Element>() != null && transform.parent.GetComponent<StackLayout>().elements.Contains(GetComponent<Element>())) return;

        BuildStack();
    }

}












}

