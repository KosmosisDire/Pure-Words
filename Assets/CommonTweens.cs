using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.StackLayout;

public class CommonTweens : MonoBehaviour
{
    public RectTransform rectTransform;
    public List<Graphic> fadeGraphic = new List<Graphic>();
    public List<Graphic> colorGraphic = new List<Graphic>();
    public Color startColor;
    public Color endColor;
    public Vector2 startSize;
    public Vector2 endSize;
    public Vector2 startPosition;
    public Vector2 endPosition;
    public bool disableOnFadeOut = true;
    public bool autoSetColorOnStart = true;

    public void Start()
    {
        if (autoSetColorOnStart)
        {
            colorGraphic.ForEach(g => g.color = endColor);
        }
    }


    public void FadeColorIn(float duration)
    {
        
        colorGraphic.ForEach(g => g.DOColor(endColor, duration));
    }

    public void FadeColorOut(float duration)
    {
        colorGraphic.ForEach(g => g.DOColor(startColor, duration));
    }

    public void FadeAlphaIn(float duration)
    {
        gameObject.SetActive(true);
        fadeGraphic.ForEach(g => g.color = new Color(g.color.r, g.color.g, g.color.b, 0));
        fadeGraphic.ForEach(g => g.DOFade(1, duration));
    }

    public void FadeAlphaOut(float duration)
    {
        fadeGraphic.ForEach(g => g.color = new Color(g.color.r, g.color.g, g.color.b, 1));
        fadeGraphic.ForEach(g => g.DOFade(0, duration));
        if(disableOnFadeOut) StartCoroutine(SetActiveAfterDelay(duration, false));
    }

    public void FadeAlphaInTolerant(float duration)
    {
        gameObject.SetActive(true);
        fadeGraphic.ForEach(g => g.DOFade(1, duration));
    }

    public void FadeAlphaOutTolerant(float duration)
    {
        fadeGraphic.ForEach(g => g.DOFade(0, duration));
        if (disableOnFadeOut) StartCoroutine(SetActiveAfterDelay(duration, false));
    }

    // couroutine to set active after delay
    public IEnumerator SetActiveAfterDelay(float delay, bool active)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(active);
    }
    

    public void ScaleIn(float duration)
    {
        rectTransform.DOSizeDelta(endSize, duration);
    }

    public void ScaleOut(float duration)
    {
        rectTransform.DOSizeDelta(startSize, duration);
    }

    public void MoveIn(float duration)
    {
        rectTransform.DOMove(endPosition, duration);
    }

    public void MoveOut(float duration)
    {
        rectTransform.DOMove(startPosition, duration);
    }

}
