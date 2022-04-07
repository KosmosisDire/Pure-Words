using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.StackLayout;

public class PlayerStats : MonoBehaviour
{
    public ThemedColorTween turnColorTween;
    public float turnWidthMultiplier = 1.1f;
    public Graphic background;
    public Element element;
    public TMP_Text usernameText;
    public TMP_Text scoreText;
 
    public int Score {get => int.Parse(scoreText.text); set => scoreText.text = value.ToString();}
    public string Username {get => usernameText.text; set => usernameText.text = value;}

    readonly List<Word> playedWords = new List<Word>();

    public void AddWord(Word word)
    {
        playedWords.Add(word);
    }

    public void TurnStart()
    {
        turnColorTween.PlayForward(0.5f, true);
    }

    public void TurnEnd()
    {
        turnColorTween.PlayBackward(0.5f, true);
    }



}
