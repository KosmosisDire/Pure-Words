using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    public Vector3 activeOffset;



    public enum SpacingMode
    {
        Fixed,
        ProportionalToSelf,
        ProportionalToParent,
        ProportionalToScreen
    }

    public SpacingMode spacingMode;

    public UnityEvent OnShow;
    public UnityEvent OnShowCompleted;
    public UnityEvent OnHide;
    public UnityEvent OnHideCompleted;

    //local
    public Vector3 ActiveOffset
    {
        get
        {
            return spacingMode switch
            {
                SpacingMode.Fixed => activeOffset,
                SpacingMode.ProportionalToSelf => activeOffset.MultiplyComponent(GetComponent<RectTransform>().rect.size),
                SpacingMode.ProportionalToParent => activeOffset.MultiplyComponent(transform.parent.GetComponent<RectTransform>().rect.size),
                SpacingMode.ProportionalToScreen => activeOffset.MultiplyComponent(new Vector2(Screen.width, Screen.height)),
                _ => activeOffset,
            };
        }
    }

    //global
    public Vector3 GlobalActiveOffset
    {
        get
        {
            return spacingMode switch
            {
                SpacingMode.Fixed => activeOffset,
                SpacingMode.ProportionalToSelf => activeOffset.MultiplyComponent(GetComponent<RectTransform>().GetWorldSpaceSize()),
                SpacingMode.ProportionalToParent => activeOffset.MultiplyComponent(transform.parent.GetComponent<RectTransform>().GetWorldSpaceSize()),
                SpacingMode.ProportionalToScreen => activeOffset.MultiplyComponent(new Vector2(Screen.width, Screen.height)),
                _ => activeOffset,
            };
        }
    }
    
    [HideInInspector]
    public bool showing = false;

    //readonly CancellationTokenSource cts = new CancellationTokenSource();

    public void Show(int transitionTime)
    {
        if(showing) return;
        showing = true;
        transform.DOLocalMove(transform.localPosition + ActiveOffset, transitionTime/1000f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            OnShowCompleted.Invoke();
        });

        OnShow.Invoke();
    }

    public async void Peek(int stayDuration, int transitionTime)
    {

        Show(transitionTime);

        // try{
        //     await Task.Delay((int)((duration + transitionTime) * 1000), cts.Token);
        // } 
        // catch(TaskCanceledException){
        //     return;
        // }

        await Task.Delay(stayDuration + transitionTime);

        Hide(transitionTime);
    }

    public void Hide(int transitionTime)
    {
        if(!showing) return;

        transform.DOLocalMove(transform.localPosition - ActiveOffset, transitionTime/1000f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            OnHideCompleted.Invoke();
        });
        OnHide.Invoke();
        showing = false;
    }

    // public void OnApplicationQuit()
    // {
    //     cts.Cancel();
    // }

    

    public void OnDrawGizmosSelected()
    {
        float div = 10;
        for(int i = 0; i < div; i++)
        {
            Gizmos.DrawRay(transform.position + GlobalActiveOffset * i / div, GlobalActiveOffset/div);
        }

        Gizmos.DrawWireCube((Vector3)GetComponent<RectTransform>().GetWorldSpaceCenter() + GlobalActiveOffset, GetComponent<RectTransform>().GetWorldSpaceSize());
    }
}
