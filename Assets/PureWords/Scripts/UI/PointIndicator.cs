using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointIndicator : MonoBehaviour
{
    public int points;
    public TMP_Text pointsText;
    public SpriteRenderer backgroundSprite;

    public void Start()
    {
        pointsText.text = points.ToString();
    }

    public void UpdatePoints(int points)
    {
        this.points = points;
        pointsText.text = this.points.ToString();
    }

    public void SetColor(Color newColor)
    {
        backgroundSprite.color = newColor;
    }

    public void SetSortingOrder(int newOrderIndex)
    {
        backgroundSprite.sortingOrder = newOrderIndex;
        pointsText.GetComponent<MeshRenderer>().sortingOrder = newOrderIndex + 1;
    }

    public void IncrementSortingOrder(int by)
    {
        backgroundSprite.sortingOrder += by;
        pointsText.GetComponent<MeshRenderer>().sortingOrder += by;
    }
}
