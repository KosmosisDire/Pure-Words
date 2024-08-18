using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace UnityEngine.UI.StackLayout
{






[Serializable]
public class Section
{
    #region Enums and Variables
    public List<Element> elements = new List<Element>();
    public Element intermediateFirst = null;
    public Element intermediateLast = null;
    public Section sectionBefore = null;
    public Section sectionAfter = null;
    public StackLayout layout = null;

    public float ElementHeight => layout.Vertical ? ((GetSectionSpace().y - (layout.overflowBehavior != Overflow.Scale ? 0 : layout.GlobalSpacing * (elements.Count - 1))) / elements.Count) : layout.NormalElementSize.y;
    public float ElementWidth => layout.Vertical ? layout.NormalElementSize.x : ((GetSectionSpace().x - (layout.overflowBehavior != Overflow.Scale ? 0 : layout.GlobalSpacing * (elements.Count - 1))) / elements.Count);

    public Section(StackLayout layout, Section sectionBefore)
    {
        this.layout = layout;
        this.sectionBefore = sectionBefore;
    }

    
    public Element this[int index]
    {
        get
        {
            return elements[index];
        }
    }

    #endregion

    public void Add(Element element)
    {
        elements.Add(element);
    }

    public Vector2 GetEndingPixelOffset()
    {
        float xMax = 0;
        float yMax = 0;

        foreach (Element element in elements)
        {
            Vector2 offset = element.rectTransform.GetBottomRightOffset() + (element.HasNestedLayout ? element.nestedLayout.PixelOverflow : Vector2.zero);
            xMax = Mathf.Max(xMax, offset.x);
            yMax = Mathf.Max(yMax, offset.y);
        }

        if (intermediateFirst != null)
        {
            Vector2 offset = intermediateFirst.rectTransform.GetBottomRightOffset() + (intermediateFirst.HasNestedLayout ? intermediateFirst.nestedLayout.PixelOverflow : Vector2.zero);
            xMax = Mathf.Max(xMax, offset.x);
            yMax = Mathf.Max(yMax, offset.y);
        }

        if (intermediateLast != null)
        {
            Vector2 offset = intermediateLast.rectTransform.GetBottomRightOffset() + (intermediateLast.HasNestedLayout ? intermediateLast.nestedLayout.PixelOverflow : Vector2.zero);
            xMax = Mathf.Max(xMax, offset.x);
            yMax = Mathf.Max(yMax, offset.y);
        }

        return new Vector2(xMax, yMax);
    }

    public Vector2 GetSectionSpace()
    {
        Vector2 startingPosition = Vector2.zero;
        Vector2 endingPosition = layout.Size;

        if (sectionBefore != null)
        {
            startingPosition = sectionBefore.GetEndingPixelOffset();
        }

        if(intermediateLast != null)
        {
            endingPosition = intermediateLast.rectTransform.GetTopLeftOffset() + (intermediateLast.HasNestedLayout ? intermediateLast.nestedLayout.PixelOverflow : Vector2.zero);
        }

        Vector2 SectionSpace = endingPosition - startingPosition;
        float ElementHeight = layout.Vertical ? ((SectionSpace.y - (layout.overflowBehavior != Overflow.Scale ? 0 : layout.GlobalSpacing * (elements.Count - 1))) / elements.Count) : layout.NormalElementSize.y;
        float ElementWidth = layout.Vertical ? layout.NormalElementSize.x : ((SectionSpace.x - (layout.overflowBehavior != Overflow.Scale ? 0 : layout.GlobalSpacing * (elements.Count - 1))) / elements.Count);

        Vector2 spaceTakenByManualElements = Vector2.zero;
        Vector2 spaceTakenByElementwiseSpacing = Vector2.zero;
        for (int i = 0; i < elements.Count; i++)
        {
            Element element = elements[i];
            if(!element.autoHeight)
            {
                spaceTakenByManualElements += Vector2.up * (element.rectTransform.rect.height - ElementHeight)*2;
            }

            if(!element.autoWidth)
            {
                spaceTakenByManualElements += Vector2.right * (element.rectTransform.rect.width - ElementWidth)*2;
            }

            if(layout.Vertical) spaceTakenByElementwiseSpacing += Vector2.up * element.ExtraSpacing * 2;
            else spaceTakenByElementwiseSpacing += Vector2.right * element.ExtraSpacing * 2;
        }

        

        return endingPosition - startingPosition - spaceTakenByManualElements - spaceTakenByElementwiseSpacing;
    }

    public bool BuildStack()
    {
        Vector3 startingOffset = Vector3.zero;
        if(sectionBefore != null) startingOffset = sectionBefore.GetEndingPixelOffset();

        float xPos = (layout.Horizontal ? startingOffset.x : 0) + layout.Margins.left;
        float yPos = (layout.Horizontal ? 0 : startingOffset.y) + layout.Margins.top;

        List<Element> elementsToCheck = new List<Element>();
        elementsToCheck.AddRange(elements);
        if(intermediateFirst != null) elementsToCheck.Add(intermediateFirst);
        if(intermediateLast != null) elementsToCheck.Add(intermediateLast);

        foreach (Element element in elementsToCheck)
        {
            if(element != intermediateFirst &&  element != intermediateLast && (layout.Horizontal && !element.autoPosX || layout.Vertical && !element.autoPosY))
            {
                layout.BuildSections();
                return false;
            } 

            if(element.HasNestedLayout) element.nestedLayout.BuildStack();

            Vector2 size = new Vector2
            (
                element.autoWidth ? ElementWidth : element.rectTransform.rect.width, 
                element.autoHeight ? ElementHeight : element.rectTransform.rect.height
            );


            if(size.x < size.y) size.y = Mathf.Lerp(size.y, size.x, element.squarness);
            else size.x = Mathf.Lerp(size.x, size.y, element.squarness);
            

            element.rectTransform.SetSizeInPixels(size.x, size.y);

            if(element.autoPosX && element.autoPosY) element.rectTransform.SetTopLeftOffset(xPos, yPos);
            else if(element.autoPosX) element.rectTransform.SetTopLeftOffset(RectTransform.Axis.Horizontal, xPos);
            else if(element.autoPosY) element.rectTransform.SetTopLeftOffset(RectTransform.Axis.Vertical, yPos);

        
            if(layout.Horizontal) xPos += size.x + layout.GlobalSpacing + element.ExtraSpacing;
            if(layout.Vertical) yPos += size.y + layout.GlobalSpacing + element.ExtraSpacing;
        }

        return true;
    }

}






}