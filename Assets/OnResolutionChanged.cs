using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class OnResolutionChanged : MonoBehaviour
{
    
    public UnityEvent onResolutionChanged;
    new public bool runInEditMode = false;
    public bool runInPlayMode = true;
    public Vector2Int lastResolution;

    // Update is called once per frame
    void Update()
    {
        if(lastResolution == Vector2Int.zero) lastResolution = new Vector2Int(Screen.width, Screen.height);

        if((runInEditMode && !Application.isPlaying) || (runInPlayMode && Application.isPlaying))
        {
            Vector2Int resolution = new Vector2Int(Screen.width, Screen.height);
            if(resolution != lastResolution)
            {
                lastResolution = resolution;
                onResolutionChanged.Invoke();
            }
        }
    }
}
