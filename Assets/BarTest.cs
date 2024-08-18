using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarTest : MonoBehaviour
{
    void Update()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 parentsize = rt.parent.GetComponent<RectTransform>().rect.size;
        rt.SetSizeInPixels(parentsize.x/3f, parentsize.y/30f);
        rt.SetBottomLeftOffset(parentsize.x/3f, parentsize.y/25f);
        
    }
}
