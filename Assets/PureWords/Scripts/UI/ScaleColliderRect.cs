using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleColliderRect : MonoBehaviour
{
    public List<RectTransform> boundObjects = new List<RectTransform>();
    public float margin = 0.1f;


    public (Vector2 size, Vector2 center) GetBounds()
    {
        Vector2 min = Vector2.zero * float.MaxValue;
        Vector2 max = Vector2.one * float.MinValue;

        foreach (RectTransform rect in boundObjects)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 rectMin = corners[0];
            Vector2 rectMax = corners[2];

            min = Vector2.Min(min, rectMin);
            max = Vector2.Max(max, rectMax);
        }
        min -= Vector2.one * margin;
        max += Vector2.one * margin;

        return (max - min, (max + min) / 2);
    }


    // Update is called once per frame
    public void Update()
    {
        GetComponent<RectTransform>().pivot = Vector2.one/2;

        (Vector2 size, Vector2 center) = GetBounds();

        GetComponent<BoxCollider2D>().size = size;
        GetComponent<BoxCollider2D>().offset = Vector2.zero;
        GetComponent<RectTransform>().position = center;
    }
}
