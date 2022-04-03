using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.StackLayout;
public class Rules
{
    public static bool useInLine = true;
    public static bool useIsConnectedToPreviousMoves = true;
    public static bool useIsConnectedToSelf = true;
    public static bool useIsDictionaryMove = true;
    public static bool anychronousTurns = false;

    public static bool allowProperNouns = false;
    public static bool allowNames = false;
    public static bool allowAbbreviations = false;

    //returns whether the user placed tiles are all in a straight line
    public static (bool isInLine, Vector2 direction) InLine()
    {
        if(Board.instance.userPlacedTiles.Count == 1)
        {
            var neighbors = Board.instance.GetNeighbors(Board.instance.userPlacedTiles[0]);
            if(neighbors.Length > 0)
            {
                var diff = neighbors[0].placedSpace.coordinates - Board.instance.userPlacedTiles[0].placedSpace.coordinates;
                return (true, new Vector2(Mathf.Abs(diff.x), Mathf.Abs(diff.y)));
            }

            return (true, Vector2Int.right);
        }

        float xAvg = Board.instance.userPlacedTiles[0].placedSpace.coordinates.x;
        float yAvg = Board.instance.userPlacedTiles[0].placedSpace.coordinates.y;
        for (int i = 0; i < Board.instance.userPlacedTiles.Count; i++)
        {
            xAvg += Board.instance.userPlacedTiles[i].placedSpace.coordinates.x;
            yAvg += Board.instance.userPlacedTiles[i].placedSpace.coordinates.y;

            xAvg /= 2;
            yAvg /= 2;

            if(Board.instance.userPlacedTiles[i].placedSpace.coordinates.x != xAvg && Board.instance.userPlacedTiles[i].placedSpace.coordinates.y != yAvg)
                return (false, Vector2.zero);
        }

        return (true, xAvg == Board.instance.userPlacedTiles[0].placedSpace.coordinates.x ? Vector2.up : Vector2.right);
    }

    public static bool IsConnectedToPreviousMoves()
    {
        if(!useIsConnectedToPreviousMoves) return true;
        if(Board.instance.userPlacedTiles.Count == Board.instance.totalTilesOnBoard)
            return true;

        for(int i = 0; i < Board.instance.userPlacedTiles.Count; i++)
        {
            Tile tile = Board.instance.userPlacedTiles[i];
            Tile[] neighbors = Board.instance.GetNeighbors(tile);
            for (int j = 0; j < neighbors.Length; j++)
            {
                if(!Board.instance.userPlacedTiles.Contains(neighbors[j])) return true;
            }
        }

        return false;
    }

    public static bool IsConnectedToSelf(Word word)
    {
        if(!useIsConnectedToSelf) return true;
        if(Board.instance.userPlacedTiles.Count == 1)
            return true;

        List<Word> words = new List<Word>{word};
        words.AddRange(word.SecondaryWords);

        List<Tile> foundTiles = new List<Tile>();

        for(int i = 0; i < words.Count; i++)
        {
            Tile[] tiles = words[i].tiles;
            for(int j = 0; j < tiles.Length; j++)
            {
                Tile t = tiles[j];
                if(!foundTiles.Contains(t) && Board.instance.userPlacedTiles.Contains(t))
                {
                    foundTiles.Add(t);
                }
            }
        }

        if(foundTiles.Count == Board.instance.userPlacedTiles.Count)
        {
            return true;
        }

        return false;
    }

    public static string[] dictionaryWords;
    public static string[] properNouns;
    public static string[] names;
    public static string[] abbreviations;
    public async static void BuildDictionary()
    {
        if(dictionaryWords == null)
        {
            var wordsTextFileRequest = Resources.LoadAsync<TextAsset>("ScrabbleWords");
            var properNounsFileRequest = Resources.LoadAsync<TextAsset>("ProperNouns");
            var namesFileRequest = Resources.LoadAsync<TextAsset>("Names");
            var abbreviationsFileRequest = Resources.LoadAsync<TextAsset>("Abbreviations");

            await TaskExt.WaitUntil(() => wordsTextFileRequest.isDone && properNounsFileRequest.isDone && namesFileRequest.isDone && abbreviationsFileRequest.isDone, 100, 5000);

            var wordsTextFile = wordsTextFileRequest.asset as TextAsset;
            var properNounsFile = properNounsFileRequest.asset as TextAsset;
            var namesFile = namesFileRequest.asset as TextAsset;
            var abbreviationsFile = abbreviationsFileRequest.asset as TextAsset;

            dictionaryWords = wordsTextFile.text.Split('\r', '\n');
            properNouns = properNounsFile.text.Split('\r', '\n');
            names = namesFile.text.Split('\r', '\n');
            abbreviations = abbreviationsFile.text.Split('\r', '\n');
        }
    }

    public static bool IsDictionaryWord(string word)
    {
        if(dictionaryWords.Length == 0) throw new Exception("Run \"Rules.BuildDictionary()\" before checking dictionary");
        if(dictionaryWords.Contains(word)) return true;
        else if (allowProperNouns && properNouns.Contains(word)) return true;
        else if (allowNames && names.Contains(word)) return true;
        else if (allowAbbreviations && abbreviations.Contains(word)) return true;
        else return false;
    }

    public static bool IsDictionaryMove(Word word)
    {
        if(!useIsDictionaryMove) return true;

        List<Word> words = new List<Word>{word};
        words.AddRange(word.SecondaryWords);

        for(int i = 0; i < words.Count; i++)
        {
            if(! IsDictionaryWord(words[i].letters))
                return false;
        }

        return true;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int points = 0;

    [Header("Points Indicator")]
    public PointIndicator pointsIndicatorPrefab;
    public Color validPoints;
    public Color invalidPoints;
    public Color placedPoints;
    [Space(15)]
    public Toast invalidMoveToast;
    public Toast notTurnToast;

    [Header("Word Placement Indicator")]
    public SpriteRenderer wordPlacementIndicatorPrefab;
    public Color validWordPlacement;
    public Color invalidWordPlacement;
    public Color otherUserWordPlacement;
    public float wordIndicatorMargin = 0.1f;

    [Header("Players")]
    public StackLayout playerStatsList;
    public PlayerStats playerStatsPrefab;
    public bool isMyTurn = false;
    readonly Dictionary<string, PlayerStats> playerStatistics = new Dictionary<string, PlayerStats>();
    public string turnUsername;

    public bool Paused { get; private set; }

    PointIndicator pointsIndicator;
    Word currentWord;
    Move lastMove;
    

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);

        pointsIndicator = Instantiate(pointsIndicatorPrefab);
        pointsIndicator.gameObject.SetActive(false);
        Rules.BuildDictionary();
    }

    public void PauseGame()
    {
        Paused = true;
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Paused = false;
        Time.timeScale = 1;
    }

    public (bool validMove, Move? move) CheckBoard()
    {
        if(currentWord != null) currentWord.HidePlacementIndicator(true);
        pointsIndicator.gameObject.SetActive(false);


        if(Board.instance.userPlacedTiles.Count == 0) return (false, null);
        
        Move? move = null;
        bool isValid = false;
        var (isInLine, direction) = Rules.InLine();
        var connectedToPrevious = Rules.IsConnectedToPreviousMoves();

        //first rule wave
        if(isInLine && connectedToPrevious)
        {
            var wordPos = Board.instance.userPlacedTiles[0].placedSpace.coordinates;
            currentWord = Board.instance.GetWordAtPosition(wordPos, direction);
            if(currentWord != null) move = new Move(currentWord, GameNetwork.instance.Username);
        }

        if(move == null) return (isValid, null);

        var selfConnected = Rules.IsConnectedToSelf(currentWord);
        if(!selfConnected) return (isValid, move);

        isValid = true;
        var isDictionary = Rules.IsDictionaryMove(currentWord);
        if(isDictionary)
        { 
            pointsIndicator.SetColor(validPoints);
        }
        else
        {
            isValid = false;
            pointsIndicator.SetColor(invalidPoints);
        }

        pointsIndicator.gameObject.SetActive(true);
        pointsIndicator.transform.position = currentWord.tiles.Last().transform.position + new Vector3(0.5f, 0.3f, -0.1f); 
        pointsIndicator.UpdatePoints(move.Value.score);

        //show placement indicators
        currentWord.ShowPlacementIndicator(validWordPlacement, invalidWordPlacement, wordIndicatorMargin, true);

        return (isValid, move);
    }

    public void AddPlayerToGame(string username)
    {
        if(playerStatistics.ContainsKey(username)) return;

        var playerStats = Instantiate(playerStatsPrefab);
        playerStats.Username = username;
        playerStats.transform.SetParent(playerStatsList.transform, false);
        playerStatistics.Add(username, playerStats);
        playerStats.Score = 0;
    }

    public void AddToPlayerScore(string username, int add)
    {
        if(!playerStatistics.ContainsKey(username))
        {
            AddPlayerToGame(username);
        }

        playerStatistics[username].Score += add;
        if(username == GameNetwork.instance.Username) points += add;
    }

    public void SetPlayerTurn(string username)
    {
        Debug.Log("Turn: " + username);
        isMyTurn = username == GameNetwork.instance.Username;
        if(!string.IsNullOrEmpty(turnUsername))
            playerStatistics[turnUsername].TurnEnd();

        turnUsername = username;
        playerStatistics[turnUsername].TurnStart();

        //vibrate phone if it's my turn
        if(isMyTurn)
        {
            Handheld.Vibrate();
        }
    }

    public void HandleOtherPlayerMove(Move m)
    {
        lastMove.word.HidePlacementIndicator(false);
        lastMove = m;
        m.word.ShowPlacementIndicator(otherUserWordPlacement, wordIndicatorMargin);
        Board.instance.CreatePermenantWord(m.word);
        instance.AddToPlayerScore(m.username, m.score);
        IndicatePlacedWord();
    }

    public void IndicatePlacedWord()
    {
        pointsIndicator.SetColor(placedPoints);
        pointsIndicator.gameObject.SetActive(true);
        pointsIndicator.transform.position = lastMove.word.tiles.Last().transform.position + new Vector3(0.5f, 0.3f, -0.1f);
        pointsIndicator.UpdatePoints(lastMove.score);
    }


    public void SubmitCurrentBoard()
    {
        var (validMove, move) = CheckBoard();

        if(validMove && isMyTurn && GameNetwork.instance.online)
        {
            TileTray.instance.Save();
            if(!Rules.anychronousTurns) isMyTurn = false;
            GameNetwork.instance.SendMove(move.Value);
            Board.instance.Commit();
            TileTray.instance.ReplenishTiles();
            AddToPlayerScore(move.Value.username, move.Value.score);
            move.Value.word.HidePlacementIndicator(true);
            lastMove = move.Value;
            IndicatePlacedWord();
            if(GameNetwork.instance.online) TileTray.instance.Save();
        }
        else if(!validMove)
        {
            invalidMoveToast.Show(1.5f, 0.2f);
        }
        else
        {
            notTurnToast.Show(1.5f, 0.2f);
        }
    }

}
