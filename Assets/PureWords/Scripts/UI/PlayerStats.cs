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
    public Color turnColor = Color.white;
    public Color normalColor = Color.white;
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
        background.DOColor(turnColor, 0.5f);
    }

    public void TurnEnd()
    {
        background.DOColor(normalColor, 0.5f);
    }



}
