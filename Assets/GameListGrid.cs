using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class GameListGrid : GridLayout
{
    public GameTile tilePrefab;

    void Start()
    {
        if(Application.isPlaying)
            Load();
    }

    public GameTile GetGame(string code)
    {
        foreach (var tile in elements)
        {
            if (tile.GetComponent<GameTile>().code == code)
            {
                return tile.GetComponent<GameTile>();
            }
        }

        return null;
    }
    public GameTile AddGame(string code)
    {
        var tile = Instantiate(tilePrefab, transform);
        AddItem(tile.gameObject);
        tile.codeText.SetText(code.ToString());
        tile.code = code;
        Save();
        return tile;
    }

    public void DeleteGame(string code)
    {
        var tile = GetGame(code);
        if (tile == null) return;
        RemoveItem(tile.gameObject);
        Save();
    }

    public void AddPlayerToGame(string username, string code)
    {
        var game = GetGame(code);
        if(game == null)
        {
            game = AddGame(code);
        }
        game.AddPlayer(username);
        Save();
    }

    public void Save()
    {
        var games = string.Join(":",elements.Select(e => e.GetComponent<GameTile>().Serialize()));
        PlayerPrefs.SetString("SavedGames", games);
    }

    public void Load()
    {
        if(!PlayerPrefs.HasKey("SavedGames")) return;
        var games = PlayerPrefs.GetString("SavedGames");

        if(string.IsNullOrWhiteSpace(games)) return;

        var tiles = games.Split(':');
        foreach (var tile in tiles)
        {
            var newTile = Instantiate(tilePrefab, transform);
            newTile.Deserialize(tile);
            AddItem(newTile.gameObject);
        }
    }

}
