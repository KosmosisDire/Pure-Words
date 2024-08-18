using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.StackLayout;



[Serializable]
public class ThemedColorTween
{
    public Graphic graphic;
    [SerializeField]
    ThemedColor startColor;
    [SerializeField]
    ThemedColor endColor;

    public Color StartColor => startColor.Color;
    public Color EndColor => endColor.Color;

    public void PlayForward(float transitionTime, bool setInitialColor = false)
    {
        if(setInitialColor) graphic.color = startColor.Color;
        graphic.DOColor(endColor.Color, transitionTime);
    }

    public void PlayBackward(float transitionTime, bool setInitialColor = false)
    {
        if(setInitialColor) graphic.color = endColor.Color;
        graphic.DOColor(startColor.Color, transitionTime);
    }
}

public class CommonTweens : MonoBehaviour
{
    public RectTransform rectTransform;
    public List<Graphic> fadeGraphic = new List<Graphic>();
    public CanvasGroup fadeGroup;
    public List<ThemedColorTween> colorTweens = new List<ThemedColorTween>();
    public Vector2 startSize;
    public Vector2 endSize;
    public Vector2 startPosition;
    public Vector2 endPosition;
    public bool disableOnFadeOut = true;

    public void FadeAlphaInTolerant(float duration)
    {
        gameObject.SetActive(true);
        fadeGraphic.ForEach(g => 
        {
            if(g != null)
            {
                g.DOKill(true); 
                g.DOFade(1, duration);
            }
        });
        
        if(fadeGroup != null)
        { 
            fadeGroup.DOKill(true);
            fadeGroup.DOFade(1, duration);
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = true;
        }
    }

    public void FadeAlphaOutTolerant(float duration)
    {
        fadeGraphic.ForEach(g => 
        {
            if(g != null)
            {
                g.DOKill(true);
                g.DOFade(0, duration);
            }
        });
        
        if(fadeGroup != null)
        { 
            fadeGroup.DOKill(true);
            fadeGroup.DOFade(0, duration);
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

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
