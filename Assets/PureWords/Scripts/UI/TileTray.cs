using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TileTray : MonoBehaviour
{
    [HideInInspector] public List<Tile> letterTiles = new List<Tile>();
    [HideInInspector] public Tile selectedTile;
    public RectTransform tray;
    public Collider2D blockingCollider;
    new public Rigidbody2D rigidbody;
    public int traySize; 
    public TileBag bag;
    public static TileTray instance;
    [HideInInspector] public float ScreenWidth => Camera.main.orthographicSize * 2 * ((float)Screen.width / Screen.height);
    public Vector3 TargetUITileSize => Vector3.Min(Vector3.one * (ScreenWidth-0.5f) / letterTiles.Count, Vector3.one * (ScreenWidth) / 6);
    // Start is called before the first frame update
    public void Start()
    {
        bag = new TileBag();
        bag.Fill();
        if(instance == null) instance = this;
        else Destroy(gameObject);

        if(tray == null)
            tray = GetComponent<RectTransform>();
        if(rigidbody == null)
            rigidbody = tray.GetComponent<Rigidbody2D>();
        
        rigidbody.bodyType = RigidbodyType2D.Static;
    }

    public Tile NewTile(bool vowel = false, char letter = ' ')
    {
        Tile tile = Instantiate
        (
            Board.instance.tilePrefab, 
            new Vector3
            (
                Random.Range(-ScreenWidth / 2 + TargetUITileSize.x/2 + 0.1f, ScreenWidth / 2 - TargetUITileSize.x/2 - 0.1f ), 
                tray.position.y, 
                Board.instance.tilePrefab.transform.position.z
            ), 
            Quaternion.identity
        );
        
        if(vowel && letter == ' ') tile.Init(bag.DrawVowel());
        else if(letter == ' ') tile.Init(bag.DrawTile());
        else tile.Init(letter);
        
        return tile;
    }

    public void ReplenishTiles()
    {
        int newTilecount = Mathf.Max(Mathf.Min(traySize - (letterTiles.Count + Board.instance.userPlacedTiles.Count), bag.availableTiles.Count), 0);
        if(newTilecount == 0) return;

        Tile[] tempTray = new Tile[newTilecount];
        for (int i = 0; i < newTilecount-1; i++)
        {
            tempTray[i] = NewTile();
        }
    
        if(!ContainsVowel()) tempTray[newTilecount-1] = NewTile(true);
        else tempTray[newTilecount-1] = NewTile();

        //move new letters to the tray
        for (int i = 0; i < tempTray.Length; i++)
        {
            tempTray[i].MoveToTray(false);
        }

        UpdateTileScale();
        SendToServer();
        Save();
    }

    public void SendToServer()
    {
        GameNetwork.instance.SendTilebagState(bag);
    }

    public void Save()
    {
        PlayerPrefs.SetString("Tray" + GameNetwork.instance.GameCode.ToString(), Serialize());
    }

    public bool Load()
    {
        if(letterTiles.Count > 0) return false;
        bool load = PlayerPrefs.HasKey("Tray" + GameNetwork.instance.GameCode.ToString());
        if(load)
            Deserialize(PlayerPrefs.GetString("Tray" + GameNetwork.instance.GameCode.ToString()));
        return load;
    }

    public bool ContainsVowel()
    {
        bool hasVowel = false;
        foreach(Tile tile in letterTiles)
        {
            if(tile.letter == 'A' || tile.letter == 'E' || tile.letter == 'I' || tile.letter == 'O' || tile.letter == 'U')
            {
                hasVowel = true;
                break;
            }
        }

        return hasVowel;
    }

    public void UpdateTileScale(float tweenDuration = 0.4f)
    {
        for (int i = 0; i < letterTiles.Count; i++)
        {
            Tile tile = letterTiles[i];
            tile.transform.DOScale(TargetUITileSize, tweenDuration);
        }

        //set tray rect top to fit the tiles
        Canvas c = FindObjectOfType<Canvas>();
        tray.DOSizeDelta(new Vector2(tray.sizeDelta.x, TargetUITileSize.x * 1/c.transform.localScale.x * 1.7f), tweenDuration);
        
        UpdateTileLimits();
    }

    public void UpdateTileLimits()
    {
        for (int i = 0; i < letterTiles.Count; i++)
        {
            Tile tile = letterTiles[i];
            if(!tile.GetComponent<SliderJoint2D>()) continue;
            
            tile.GetComponent<SliderJoint2D>().limits = new JointTranslationLimits2D()
            {
                min = -ScreenWidth / 2 + TargetUITileSize.x/2 + 0.1f,
                max = ScreenWidth / 2 - TargetUITileSize.x/2 - 0.1f 
            };
        }
    }

    public string Serialize()
    {
        string serialized = "";
        foreach(Tile tile in letterTiles)
        {
            serialized += tile.letter;
        }

        foreach(Tile tile in Board.instance.userPlacedTiles)
        {
            serialized += tile.letter;
        }

        return serialized;
    }

    public void Deserialize(string serialized)
    {
        for (int i = 0; i < serialized.Length; i++)
        {
            Tile tile = NewTile(false, serialized[i]);
            tile.MoveToTray(true);
        }
    }
}

