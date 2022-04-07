using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class EditorBackground : MonoBehaviour
{
    public Color backgroundColor = Color.grey;
    void Update()
    {
        if(Application.isPlaying) return;

        EditorPrefs.SetString("Scene/Background", $"Scene/Background;{backgroundColor.r};{backgroundColor.g};{backgroundColor.b};{0}");
        
    }
}
