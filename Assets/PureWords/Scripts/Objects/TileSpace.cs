using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileSpace : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Vector2Int coordinates;
    public Color color;
    public Color hoverColor;
    public bool hovering = false;
    public bool mouseDown = false;
    public Tile tile = null;
    public int wordMultiplier = 1;
    public int letterMultiplier = 1;


    // Start is called before the first frame update
    public void Start()
    {
        spriteRenderer.color = color;
    }

    public Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(Mathf.Lerp(a.r, b.r, t), Mathf.Lerp(a.g, b.g, t), Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
    }

    // Update is called once per frame
    public void Update()
    {
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, ~LayerMask.NameToLayer("Spaces"));
        if(Input.GetMouseButtonUp(0) || Input.touchCount == 0 || (hovering && hit.collider != null && hit.collider.gameObject != gameObject))
        { 
            if(Board.instance.selectedSpace == this) Board.instance.selectedSpace = null;
            hovering = false;
        }

        if(Input.GetMouseButton(0) && Input.touchCount == 1 && !hovering){
            Vector2 distance = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
            if(distance.sqrMagnitude < 1.5f)
            {
                hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, ~LayerMask.NameToLayer("Spaces"));
                if(hit.collider != null)
                {
                    if(hit.collider.gameObject == gameObject)
                    {
                        hovering = true;
                        Board.instance.selectedSpace = this;
                    }
                }
            }
        }
        
        
        //color
        if(hovering)
            spriteRenderer.color = ColorLerp(spriteRenderer.color, hoverColor, 0.4f);
        else
            spriteRenderer.color = ColorLerp(spriteRenderer.color, color, 0.1f);
        
    }
}
