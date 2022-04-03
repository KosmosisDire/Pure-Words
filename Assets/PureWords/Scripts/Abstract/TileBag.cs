using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TileBag
{
    public static readonly Dictionary<char, int> tileCounts = new Dictionary<char, int>
    {
        {'A', 8},
        {'B', 2},
        {'C', 3},
        {'D', 4},
        {'E', 12},
        {'F', 2},
        {'G', 2},
        {'H', 6},
        {'I', 8},
        {'J', 1},
        {'K', 1},
        {'L', 4},
        {'M', 3},
        {'N', 7},
        {'O', 8},
        {'P', 2},
        {'Q', 1},
        {'R', 6},
        {'S', 6},
        {'T', 9},
        {'U', 4},
        {'V', 1},
        {'W', 2},
        {'X', 1},
        {'Y', 2},
        {'Z', 1}
    };

    public static readonly Dictionary<char, int> tileScores = new Dictionary<char, int>
    {
        {'A', 1},
        {'B', 7},
        {'C', 3},
        {'D', 3},
        {'E', 1},
        {'F', 4},
        {'G', 4},
        {'H', 4},
        {'I', 1},
        {'J', 11},
        {'K', 8},
        {'L', 3},
        {'M', 4},
        {'N', 2},
        {'O', 1},
        {'P', 7},
        {'Q', 10},
        {'R', 2},
        {'S', 2},
        {'T', 2},
        {'U', 1},
        {'V', 8},
        {'W', 5},
        {'X', 9},
        {'Y', 6},
        {'Z', 12}
    };
    
    [SerializeField]
    public List<char> availableTiles = new List<char>();

    public void Fill()
    {
        availableTiles.Clear();
        char[] letters = tileCounts.Keys.ToArray();
        for(int i = 0; i < tileCounts.Count; i++)
        {
            char l = letters[i];

            for(int j = 0; j < tileCounts[l]; j++)
            {
                availableTiles.Add(l);
            }
        }

        Shuffle();
        Shuffle();
        Shuffle();
        Shuffle();
        Shuffle();
    }

    public void Shuffle()
    {
        List<char> tempTiles = new List<char>();
        int tileCount = availableTiles.Count;
        for(int i = 0; i < tileCount; i++)
        {
            int index = UnityEngine.Random.Range(0, availableTiles.Count);
            tempTiles.Add(availableTiles[index]);
            availableTiles.RemoveAt(index);
        }

        availableTiles = tempTiles;
    }

    public char DrawTile()
    {
        var tile = availableTiles[0];
        availableTiles.RemoveAt(0);
        return tile;
    }

    public char DrawVowel()
    {
        char tile = ' ';
        for(int i = 0; i < availableTiles.Count; i++)
        {
            tile = availableTiles[i];
            if (tile == 'A' || tile == 'E' || tile == 'I' || tile == 'O' || tile == 'U')
            {
                availableTiles.RemoveAt(i);
                break;
            }
        }

        if(tile == ' ') tile = DrawTile();
        return tile;
    }

    public string Serialize()
    {
        string state = "";
        foreach(char tile in availableTiles)
        {
            state += tile;
        }

        return state;
    }

    public void Deserialize(string state)
    {
        availableTiles.Clear();
        foreach(char tile in state)
        {
            availableTiles.Add(tile);
        }
    }
}
