using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MatchParentSize : MonoBehaviour
{
    public RectTransform parent;
    public RectTransform rectTransform;
    public Vector2 normalizedLocalPosition;
    public bool setPosition = false;
    public bool setSize = true;
    public bool runInUpdate = false;
    public bool inEditMode = false;
    public bool inPlayMode = true;

    // Update is called once per frame
    void LateUpdate()
    {
        if(!Application.isPlaying && inEditMode && runInUpdate) Set();
        if(Application.isPlaying && inPlayMode && runInUpdate) Set();
    }

    public void Set()
    {
        if (parent == null)
        {
            parent = transform.parent.GetComponent<RectTransform>();
        }
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if(setSize) rectTransform.SetSize(parent.rect.size.x, parent.rect.size.y);
        
        if(setPosition) rectTransform.position = parent.position + (Vector3)(normalizedLocalPosition * parent.GetWorldSpaceSize());
    }
}
