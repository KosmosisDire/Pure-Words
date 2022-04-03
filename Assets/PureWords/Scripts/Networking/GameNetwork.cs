using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.StackLayout;

public class GameNetwork : MonoBehaviour
{
    public static GameNetwork instance;
    public Client client;
    public int _gameCode;
    public int GameCode 
    { 
        get
        {
            if(!string.IsNullOrWhiteSpace(gameCodeInput.text))
            { 
                _gameCode = int.Parse(gameCodeInput.text);
                GameCode = _gameCode;
            }
            
            return _gameCode;
        }
        private set
        { 
            _gameCode = value;
            gameCodeInput.SetTextWithoutNotify(_gameCode.ToString());
            gameCodeText.text = _gameCode.ToString();
        }
    }

    [Header("Inputs")]
    public TMP_InputField gameCodeInput;
    public TMP_Text gameCodeText;
    public TMP_InputField usernameText;

    [Header("Waiting Room")]
    public StackLayout onlinePlayersWaitingRoom;
    public StackLayout rowPrefab;
    public List<StackLayout> rows = new List<StackLayout>();
    public Dictionary<string, RectTransform> onlinePlayersDict = new Dictionary<string, RectTransform>();
    public RectTransform onlinePlayerTilePrefab;

    public Toast disconnectToast;

    public bool online = false;

    public List<string> onlinePlayers = new List<string>();
    public string Username { get => usernameText.text; private set => usernameText.SetTextWithoutNotify(value); }

    public void Start()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        if(PlayerPrefs.HasKey("Username"))
        {
            Username = PlayerPrefs.GetString("Username");
        }

        usernameText.onValueChanged.AddListener(delegate {
            if(!string.IsNullOrWhiteSpace(usernameText.text))
            {
                PlayerPrefs.SetString("Username", usernameText.text);
            }
        });

        client = new Client(Username);
    }

    [ContextMenu("Host Game")]
    public async void HostNewGame()
    {
        if(!Application.isPlaying) return;

        int code = Random.Range(100, 1000);

        //try values until there is one that isn't taken
        //TODO: Ask the server for a new code directly
        int num = 0;
        GameCode = code;
        while (await client.HostGame(code) != true && num < 10)
        {
            code = Random.Range(100, 1000);
            GameCode = code;
            num++;
        }

        if(num == 10) 
        {
            throw new System.Exception("Could not host game, no valid codes found");
        }

        
    }

    [ContextMenu("Join Game")]
    public void JoinGame()
    {
        if(!Application.isPlaying) return;
        client.JoinGame(GameCode);
    }

    public void SendTilebagState(TileBag bag)
    {
        client.SendTilebagToServer(bag, GameCode);
    }

    public void SendMove(Move move)
    {
        client.SendMove(move, GameCode);
    }

    public void PlayerOnline(string username)
    {
        Debug.Log("Player online: " + username);
        StackLayout row = null;
        foreach(var r in rows)
        {
            if(r.elements.Count == 3) continue;
            row = r;
            break;
        }

        if(row == null)
        {
            row = Instantiate(rowPrefab, onlinePlayersWaitingRoom.transform);
            row.rectTransform.SetParent(onlinePlayersWaitingRoom.transform, false);
            row.rectTransform.localPosition = Vector3.zero;
            rows.Add(row);
        }

        var tile = Instantiate(onlinePlayerTilePrefab, row.transform);
        tile.GetComponentInChildren<TMP_Text>().text = username;
        tile.SetParent(row.transform, false);
        tile.localPosition = Vector3.zero;

        onlinePlayersDict.Add(username, tile);

        onlinePlayers.Add(username);

        if(GameManager.instance != null)
        {
            GameManager.instance.AddPlayerToGame(username);
        }
    }

    public void PlayerOffline(string username)
    {
        Debug.Log("Player offline");
        if(onlinePlayersDict.ContainsKey(username))
        {
            Destroy(onlinePlayersDict[username].gameObject);
            foreach(var r in rows)
            {
                r.BuildSections();
            }
            onlinePlayersDict.Remove(username);
        }
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected");
        online = false;
        disconnectToast.Show(1000);
        StartCoroutine(LoadSceneDelay(0, 5000));
    }

    //create coroutine to load scene after a delay
    public IEnumerator LoadSceneDelay(int scene, int delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(scene);
    }

    public void OnReconnected()
    {
        disconnectToast.Hide(1);
        online = true;
    }

    public void OnApplicationQuit()
    {
        client.BeginFullDisconnect();
    }
}
