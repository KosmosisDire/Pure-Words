using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TextDisplay : MonoBehaviour
{
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    int maxLetters;
    public bool pad;
    [SerializeField]
    string textString;

    public void SetText(string newText)
    {
        textString = newText;
        text.text = textString.Substring(0, Mathf.Min(textString.Length, maxLetters));
        if(pad) text.text = text.text.PadLeft(maxLetters);
    }
}
