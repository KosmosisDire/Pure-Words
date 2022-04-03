using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Word
{
    public string letters;
    public Vector2Int startingSpace;
    public bool horizontal;
    public Tile[] tiles;
    Word[] secondaryWords;
    public Word[] SecondaryWords 
    { 
        get 
        {
            if(secondaryWords == null)
                secondaryWords = Board.instance.GetSecondaryWords(this, true);
            return secondaryWords;
        } 
    }
    public SpriteRenderer indicator;
    public Word(string letters, Vector2Int startingSpace, bool horizontal)
    {
        this.letters = letters;
        this.startingSpace = startingSpace;
        this.horizontal = horizontal;
        tiles = new Tile[0];
        
        tiles = Board.instance.GetWordTiles(this);
    }

    public int CalculateIndividualScore()
    {
        int score = 0;
        int wordMultiplier = 1;
        foreach (Tile tile in tiles)
        {
            score += tile.score * (Board.instance.userPlacedTiles.Contains(tile) ? tile.placedSpace.letterMultiplier : 1);
            wordMultiplier *= Board.instance.userPlacedTiles.Contains(tile) ? tile.placedSpace.wordMultiplier : 1;
        }

        score *= wordMultiplier;

        return score;
    }

    public void IncrementSortingOrder(int by)
    {
        foreach (Tile tile in tiles)
        {
            tile.IncrementSortingOrder(by);
        }
    }

    public void ShowPlacementIndicator(Color dictionaryValid, Color dictionaryInvalid, float margin = 0.1f, bool secondary = false)
    {
        if(indicator != null) HidePlacementIndicator(secondary);

        bool valid = Rules.IsDictionaryWord(letters);
        ShowPlacementIndicator(valid ? dictionaryValid : dictionaryInvalid, margin);

        indicator.transform.position = tiles[0].transform.position + new Vector3(-0.5f - margin, 0.5f + margin, valid ? -0.2f : -0.1f);

        if(secondary)
        {
            secondaryWords.ToList().ForEach(word => word.ShowPlacementIndicator(dictionaryValid, dictionaryInvalid, margin));
        }
    }

    public void ShowPlacementIndicator(Color color, float margin = 0.1f)
    {
        IncrementSortingOrder(1);
        indicator = GameObject.Instantiate(GameManager.instance.wordPlacementIndicatorPrefab);
        indicator.color = color;
        indicator.size = new Vector2(horizontal ? (tiles.Length + margin * 2) : (1 + margin * 2) , horizontal ? (1 + margin * 2) : (tiles.Length + margin * 2));
    }

    public void HidePlacementIndicator(bool secondary = false)
    {
        if(indicator != null)
        {
            GameObject.Destroy(indicator.gameObject);
            IncrementSortingOrder(-1);
        }

        if(secondary)
        {
            secondaryWords.ToList().ForEach(w => w.HidePlacementIndicator());
        }
    }

    public static Word FromJson(string json)
    {
        return (Word)JsonUtility.FromJson(json, typeof(Word));
    }
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    public override string ToString()
    {
        return letters;
    }
}
