using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    
    //public bool circular;
    public int diameter;
    char[,] letters;

    public List<Tile> userPlacedTiles;

    public int totalTilesOnBoard = 0;

    public Tile tilePrefab;
    public TileSpace boardSpacePrefab;
    public TileSpace TWSpacePrefab;
    public TileSpace TLSpacePrefab;
    public TileSpace DWSpacePrefab;
    public TileSpace DLSpacePrefab;
    [HideInInspector] public TileSpace selectedSpace;

    [Header("Looks")]
    public Color boardBackgroundColor;
    public static Board instance;

    [SerializeField]
    public TileSpaceDictionary spaces = new TileSpaceDictionary();

    public bool EmptySpaceSelected => selectedSpace != null && selectedSpace.tile == null;

    public List<Vector2Int> doubleLetters = new List<Vector2Int>();
    public List<Vector2Int> tripleLetters = new List<Vector2Int>();
    public List<Vector2Int> doubleWords = new List<Vector2Int>();
    public List<Vector2Int> tripleWords = new List<Vector2Int>();

    
    


    // Start is called before the first frame update
    public void OnEnable()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        if(diameter % 2 == 0)
        {
            Debug.LogWarning("Diameter must be odd, incrementing by 1");
            diameter++;
        }

        letters = new char[diameter, diameter];
        for (int i = 0; i < diameter; i++)
        {
            for (int j = 0; j < diameter; j++)
            {
                letters[i, j] = ' ';
            }
        }
        Camera.main.backgroundColor = boardBackgroundColor;
        for (int i = 0; i < diameter; i++)
        {
            for (int j = 0; j < diameter; j++)
            {
                var prefab = boardSpacePrefab;

                if(doubleLetters.Contains(new Vector2Int(i, j)))
                {
                    prefab = DLSpacePrefab;
                }
                else if(tripleLetters.Contains(new Vector2Int(i, j)))
                {
                    prefab = TLSpacePrefab;
                }
                else if(doubleWords.Contains(new Vector2Int(i, j)))
                {
                    prefab = DWSpacePrefab;
                }
                else if(tripleWords.Contains(new Vector2Int(i, j)))
                {
                    prefab = TWSpacePrefab;
                }

                // if((i % 15 == 0) && (j % 15 == 0))
                // {
                //     prefab = TWSpacePrefab;
                // }
                // else if((j == diameter - 1 || j == 0) && i == diameter / 2)
                // {
                //     prefab = TLSpacePrefab;
                // }
                // else if((i == diameter - 1 || i == 0) && j == diameter / 2 - 1)
                // {
                //     prefab = DWSpacePrefab;
                // }
                // else if((j == diameter - 1 || j == 0) && i == diameter / 2 - 1)
                // {
                //     prefab = DLSpacePrefab;
                // }

                TileSpace boardSpace = Instantiate(prefab, new Vector3(i - diameter/2, diameter/2 - j, 0), Quaternion.identity);
                boardSpace.transform.SetParent(transform);
                boardSpace.coordinates = new Vector2Int(i, j);
                spaces.Add(new Vector2Int(i, j), boardSpace);
            }
        }
    }

    public void CreatePermenantTile(char letter, Vector2Int pos)
    {
        if(pos.x > diameter || pos.y > diameter || pos.x < 0 || pos.y < 0)
        {
            Debug.LogWarning("Attempted to place letter outside of board");
            return;
        }

        if(userPlacedTiles.Any(t => t.placedSpace.coordinates == pos))
        {
            instance.RecallPlacedTilesToTray(); //recall all tiles to tray if this word is being placed in the same place
        }

        if(letters[pos.x,pos.y] != ' ')
        {
            if(letters[pos.x,pos.y] == letter)
            {
                return;
            }
            else
            {
                throw new Exception("Invalid, spot already contains a different letter!");
            }
        }

        Tile tile = Instantiate(tilePrefab, transform);
        tile.Init(letter);
        tile.SetPlacedProperties();
        tile.transform.position = (Vector3)GetTilePosition(pos.x, pos.y) + Vector3.forward * tile.transform.position.z;
        tile.movable = false;
        tile.selectionCollider.enabled = false;
        tile.IncrementSortingOrder(-4);
        totalTilesOnBoard++;
        SetSpace(pos, tile);
    }

    public void CreatePermenantWord(Word word)
    {
        int x = word.startingSpace.x;
        int y = word.startingSpace.y;

        for (int i = 0; i < word.letters.Length; i++)
        {
            CreatePermenantTile(word.letters[i], new Vector2Int(x, y));
            if (word.horizontal) x++;
            else y++;
        }
    }

    public void SetSpace(Vector2Int xy, Tile tile)
    {
        if(tile == null) letters[xy.x, xy.y] = ' ';
        else 
        {
            letters[xy.x, xy.y] = tile.letter;
            tile.placedSpace = spaces[xy];
        }

        spaces[xy].tile = tile;
    }

    public Word GetWordAtPosition(Vector2Int pos, Vector2 direction)
    {
        if(direction.x != 0 && direction.y != 0)
        {
            Debug.LogWarning("Attempted to get word at position with non-orthogonal direction");
            return null;
        }

        if(direction.x == 0 && direction.y == 0)
        {
            Debug.LogWarning("Attempted to get word at position with zero direction");
            return null;
        }

        if(pos.x < 0 || pos.x > diameter - 1 || pos.y < 0 || pos.y > diameter - 1)
        {
            Debug.LogWarning("Attempted to get word at position outside of board");
            return null;
        }

        direction.Normalize();
        Vector2Int directionInt = new Vector2Int((int)direction.x, (int)direction.y);

        if(letters[pos.x, pos.y] == ' ') return null;
        
        while(pos.x < diameter && pos.y < diameter && pos.x >= 0 && pos.y >= 0 && letters[pos.x, pos.y] != ' ')
        {
            pos += directionInt;
        }

        pos -= directionInt; //get back to the last letter of the word

        List<char> wordbuild = new List<char>();
        directionInt = -directionInt;

        while(pos.x < diameter && pos.y < diameter && pos.x >= 0 && pos.y >= 0 && letters[pos.x, pos.y] != ' ')
        {
            wordbuild.Add(letters[pos.x, pos.y]);
            pos += directionInt;
        }

        pos -= directionInt; //get back to the last letter of the word

        wordbuild.Reverse();

        if(wordbuild.Count == 0) return null;

        return new Word(new string(wordbuild.ToArray()), pos, direction.x == 1);
    }

    //returns all words connected to actively placed tiles
    public Word[] GetSecondaryWords(Word word, bool log = false)
    {
        List<Tile> wordTiles = new List<Tile>();
        wordTiles.AddRange(word.tiles);
        List<Word> secondaryWords = new List<Word>();

        for (int i = 0; i < wordTiles.Count; i++)
        {
            if(!userPlacedTiles.Contains(wordTiles[i])) continue;

            Word possibleSecond = GetWordAtPosition(wordTiles[i].placedSpace.coordinates + (word.horizontal ? Vector2Int.up : Vector2Int.right), word.horizontal ? Vector2.up : Vector2.right);
            if(possibleSecond == null) possibleSecond = GetWordAtPosition(wordTiles[i].placedSpace.coordinates - (word.horizontal ? Vector2Int.up : Vector2Int.right), word.horizontal ? Vector2.up : Vector2.right);

            if(possibleSecond != null)
            {
                foreach(var t in possibleSecond.tiles)
                {
                    if(wordTiles.Contains(t))
                    {
                        wordTiles.Remove(t);
                        i--;
                    }
                }

                secondaryWords.Add(possibleSecond);
            }
        }
        
        
        if(log){
            int i = 0;
            foreach (var w in secondaryWords)
            {
                Debug.Log(i + ": " + w.letters + " at " + w.startingSpace + " " + (w.horizontal ? "Across" : "Down"));
                i++;
            }
        }
        return secondaryWords.ToArray();
    }

    public Tile[] GetWordTiles(Word word)
    {
        List<Tile> tiles = new List<Tile>();
        for(int i = 0; i < word.letters.Length; i++)
        {
            if(word.horizontal)
            {
                
                tiles.Add(spaces[word.startingSpace + Vector2Int.right * i].tile);
            }
            else
            {
                tiles.Add(spaces[word.startingSpace + Vector2Int.up * i].tile);
            }
        }
        return tiles.ToArray();
    }

    public Tile[] GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        if(tile.placedSpace.coordinates.x > 0)
        {
            var t = spaces[tile.placedSpace.coordinates + Vector2Int.left].tile;
            if(t != null) neighbors.Add(t);

        }
        if(tile.placedSpace.coordinates.x < diameter - 1)
        {
            var t = spaces[tile.placedSpace.coordinates + Vector2Int.right].tile;
            if(t != null) neighbors.Add(t);
        }
        if(tile.placedSpace.coordinates.y > 0)
        {
            var t = spaces[tile.placedSpace.coordinates + Vector2Int.down].tile;
            if(t != null) neighbors.Add(t);
        }
        if(tile.placedSpace.coordinates.y < diameter - 1)
        {
            var t = spaces[tile.placedSpace.coordinates + Vector2Int.up].tile;
            if(t != null) neighbors.Add(t);
        }

        return neighbors.ToArray();
    }
    
    public Vector2 GetTilePosition(int x, int y)
    {
        return new Vector2(x - diameter/2, diameter/2 - y);
    }

    public void RecallPlacedTilesToTray()
    {
        for(int i = 0; i < userPlacedTiles.Count; i++)
        {
            Tile t = userPlacedTiles[i];
            t.UserPickupTile();
            t.MoveToTray();
            i--;
        }
        userPlacedTiles.Clear();
        TileTray.instance.UpdateTileScale();
    }

    public void Commit()
    {
        foreach(Tile tile in userPlacedTiles)
        {
            tile.movable = false;
            tile.selectionCollider.enabled = false;
            tile.IncrementSortingOrder(-4);
        }
        userPlacedTiles.Clear();
    }

    
}
